using AiFoundryAgent.Services;

namespace AiFoundryAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "BasicAuthentication")]
public class RagController(
    PersistentAgentsClient client,
    IOptionsMonitor<ChatApiOptions> options,
    ILogger<RagController> logger,
    SearchClient? searchClient = null,
    BlobContainerClient? blobContainerClient = null,
    IIndexerService? indexerService = null,
    IEmbeddingService? embeddingService = null,
    IDocumentProcessingService? documentProcessingService = null) : ControllerBase
{
    private readonly PersistentAgentsClient _client = client;
    private readonly IOptionsMonitor<ChatApiOptions> _options = options;
    private readonly ILogger<RagController> _logger = logger;
    private readonly SearchClient? _searchClient = searchClient;
    private readonly BlobContainerClient? _blobContainerClient = blobContainerClient;
    private readonly IIndexerService? _indexerService = indexerService;
    private readonly IEmbeddingService? _embeddingService = embeddingService;
    private readonly IDocumentProcessingService? _documentProcessingService = documentProcessingService;

    [HttpPost("threads")]
    public async Task<IActionResult> CreateThread()
    {
        try
        {
            _logger.LogInformation("Creating new RAG thread");
            
            // Create a thread for the RAG session
            PersistentAgentThread thread = await _client.Threads.CreateThreadAsync();

            return Ok(new { id = thread.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating RAG thread");
            return StatusCode(500, new { error = "An error occurred while creating the RAG thread" });
        }
    }

    [HttpPost("completions/{threadId}")]
    public async Task<IActionResult> GetCompletion([FromRoute] string threadId, [FromBody] RagCompletionRequest request)
    {
        if (string.IsNullOrWhiteSpace(threadId))
            return BadRequest(new { error = "Thread ID cannot be null or empty" });
            
        if (string.IsNullOrWhiteSpace(request?.Prompt))
            return BadRequest(new { error = "Prompt cannot be null or empty" });

        _logger.LogDebug("RAG prompt received {Prompt}", request.Prompt);
        var config = _options.CurrentValue;

        try
        {
            // Perform enhanced search combining keyword and vector search to get relevant documents
            var searchResults = await EnhancedSearchDocumentsAsync(request.Prompt);
            
            // Enhance the prompt with search results
            var enhancedPrompt = BuildEnhancedPrompt(request.Prompt, searchResults);

            // Create message with enhanced prompt
            PersistentThreadMessage message = await _client.Messages.CreateMessageAsync(
                threadId,
                MessageRole.User,
                enhancedPrompt);

            // Execute the run
            ThreadRun run = await _client.Runs.CreateRunAsync(threadId, config.RagAgentId);

            // Wait for the run to complete
            while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                run = (await _client.Runs.GetRunAsync(threadId, run.Id)).Value;
            }

            // Get the response
            Pageable<PersistentThreadMessage> messages = _client.Messages.GetMessages(
                threadId: threadId, order: ListSortOrder.Ascending);

            var fullText = messages
                .Where(m => m.Role == MessageRole.Agent)
                .SelectMany(m => m.ContentItems.OfType<MessageTextContent>())
                .LastOrDefault()?.Text ?? "No response generated";

            // Return both the answer and source documents (only if there are relevant results)
            var relevantSources = searchResults.Where(r => !string.IsNullOrEmpty(r.Title) || !string.IsNullOrEmpty(r.Summary)).ToList();
            var sourcesData = relevantSources.Select(r => new { 
                r.Title, 
                Content = r.Summary,
                r.Url,
                r.DocumentId
            }).ToList();

            return Ok(new 
            { 
                data = fullText,
                sources = sourcesData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing RAG completion request for thread {ThreadId}", threadId);
            return StatusCode(500, new { error = "An error occurred while processing the RAG request" });
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "File is required" });

        // Check if file type is allowed
        var allowedExtensions = new[] { ".pdf", ".txt", ".docx", ".doc", ".xls", ".xlsx", ".ppt", ".pptx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { error = $"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" });
        }

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
        {
            return BadRequest(new { error = "File size exceeds 10MB limit" });
        }

        try
        {
            // Check if BlobContainerClient is available
            if (_blobContainerClient == null)
            {
                _logger.LogError("BlobContainerClient is not configured, cannot upload file to Azure Blob Storage");
                return StatusCode(500, new { error = "Blob storage is not configured properly" });
            }

            // Generate a unique blob name with the original file extension
            var blobName = $"{Guid.NewGuid()}{fileExtension}";
            
            // Upload file to Azure Blob Storage with metadata
            var metadata = new Dictionary<string, string>
            {
                ["originalfilename"] = file.FileName,
                ["uploadedat"] = DateTime.UtcNow.ToString("O"),
                ["contenttype"] = file.ContentType
            };
            
            using var fileStream = file.OpenReadStream();
            var response = await _blobContainerClient.UploadBlobAsync(blobName, fileStream);
            
            // Set metadata after upload
            await _blobContainerClient.GetBlobClient(blobName).SetMetadataAsync(metadata);

            if (response != null)
            {
                _logger.LogInformation("File {FileName} uploaded to Azure Blob Storage with blob name {BlobName}", 
                    file.FileName, blobName);

                // Trigger the Azure AI Search indexer to process the uploaded document
                if (_indexerService != null)
                {
                    const string indexerName = "blob-document-indexer"; // Consistent indexer name
                    
                    // Check if indexer exists before attempting to run it
                    var indexerExists = await _indexerService.CheckIndexerExistsAsync(indexerName);
                    
                    if (!indexerExists)
                    {
                        _logger.LogError("Indexer {IndexerName} does not exist in Azure AI Search service. " +
                            "The automated setup may not have completed successfully.", indexerName);
                        
                        // Return a more informative error message to the client
                        return StatusCode(500, new { 
                            error = $"Indexer '{indexerName}' not found in Azure AI Search service. " +
                                   "The application attempted to create this automatically but the setup may have failed. " +
                                   "Please check application logs for more information."
                        });
                    }
                    
                    var indexerTriggered = await _indexerService.RunIndexerAsync(indexerName);
                    
                    if (indexerTriggered)
                    {
                        _logger.LogInformation("Successfully triggered Azure AI Search indexer for blob {BlobName}", blobName);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to trigger Azure AI Search indexer for blob {BlobName}", blobName);
                    }
                }
                else
                {
                    _logger.LogWarning("IndexerService is not available, cannot trigger Azure AI Search indexer");
                }

                // Return success response
                var blobUrl = $"{_blobContainerClient.Uri}/{blobName}";
                return Ok(new { 
                    message = "File uploaded to Azure Blob Storage and indexer triggered successfully", 
                    fileName = file.FileName,
                    blobUrl = blobUrl 
                });
            }
            else
            {
                _logger.LogError("Failed to upload file {FileName} to Azure Blob Storage", file.FileName);
                return StatusCode(500, new { error = "Failed to upload file to Azure Blob Storage" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading file {FileName} to Azure Blob Storage", file.FileName);
            return StatusCode(500, new { error = "An error occurred while uploading the file to Azure Blob Storage" });
        }
    }

    private async Task<List<SearchResult>> SearchDocumentsAsync(string query)
    {
        var results = new List<SearchResult>();
        
        // Use the fallback keyword search only
        if (_searchClient != null)
        {
            try
            {
                var searchOptions = new SearchOptions
                {
                    Size = 5, // Return top 5 results
                    IncludeTotalCount = true
                };

                var response = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    var document = result.Document;
                    var searchResult = new SearchResult
                    {
                        Title = document.ContainsKey("title") ? document["title"]?.ToString() ?? "" : "",
                        Summary = document.ContainsKey("content") ? document["content"]?.ToString() ?? "" : "",
                        Url = document.ContainsKey("url") ? document["url"]?.ToString() ?? "" : "",
                        Score = result.Score ?? 0
                    };
                    
                    // Extract blob name from metadata_storage_path if available
                    // metadata_storage_path contains the base64-encoded path of the blob
                    if (document.ContainsKey("metadata_storage_path") && document["metadata_storage_path"] != null)
                    {
                        var encodedPath = document["metadata_storage_path"]?.ToString();
                        if (!string.IsNullOrEmpty(encodedPath))
                        {
                            try
                            {
                                // Decode the base64 path to get the blob name
                                var decodedPathBytes = Convert.FromBase64String(encodedPath);
                                var decodedPath = System.Text.Encoding.UTF8.GetString(decodedPathBytes);
                                // The path is in format /container-name/blob-name
                                var pathParts = decodedPath.Split('/');
                                if (pathParts.Length >= 3)
                                {
                                    var blobName = pathParts[2]; // Get the blob name part
                                    searchResult.DocumentId = blobName; // Store the document ID directly
                                    searchResult.Url = $"/documents/{blobName}"; // Custom URL format for our download endpoint
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Could not decode metadata_storage_path: {EncodedPath}", encodedPath);
                            }
                        }
                    }
                    
                    // If DocumentId is still null, try to get it from the search result's ID field as fallback
                    if (string.IsNullOrEmpty(searchResult.DocumentId))
                    {
                        // Check if the document has an 'id' field that can be used as DocumentId
                        if (document.ContainsKey("id") && document["id"] != null)
                        {
                            var docId = document["id"]?.ToString();
                            if (!string.IsNullOrEmpty(docId))
                            {
                                searchResult.DocumentId = docId;
                            }
                        }
                    }
                    
                    // If DocumentId is still null but we have a URL, try to extract from URL
                    if (string.IsNullOrEmpty(searchResult.DocumentId) && !string.IsNullOrEmpty(searchResult.Url))
                    {
                        try
                        {
                            var uri = new Uri(searchResult.Url);
                            var path = uri.AbsolutePath;
                            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            if (pathParts.Length > 0)
                            {
                                searchResult.DocumentId = pathParts[pathParts.Length - 1]; // Last part as fallback
                            }
                        }
                        catch (UriFormatException)
                        {
                            // If URL is malformed, try simple split
                            var parts = searchResult.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0)
                            {
                                searchResult.DocumentId = parts[parts.Length - 1];
                            }
                        }
                    }
                    
                    // If primary fields are empty/meaningless, try to populate from available metadata
                    if (string.IsNullOrWhiteSpace(searchResult.Title))
                    {
                        // Try to extract title from metadata_storage_name
                        if (document.ContainsKey("metadata_storage_name") && document["metadata_storage_name"] != null)
                        {
                            searchResult.Title = document["metadata_storage_name"]?.ToString() ?? "Untitled Document";
                        }
                        else
                        {
                            // Use the document ID as title if available
                            searchResult.Title = string.IsNullOrEmpty(searchResult.DocumentId) ? "Untitled Document" : searchResult.DocumentId;
                        }
                    }
                    
                    // If content is empty/meaningless, provide a meaningful summary from available metadata
                    if (string.IsNullOrWhiteSpace(searchResult.Summary) || searchResult.Summary == "\n\n")
                    {
                        var metadataSummary = $"Document: {searchResult.Title}";
                        if (document.ContainsKey("metadata_storage_last_modified") && document["metadata_storage_last_modified"] != null)
                        {
                            metadataSummary += $"\nModified: {document["metadata_storage_last_modified"]}";
                        }
                        if (document.ContainsKey("metadata_content_type") && document["metadata_content_type"] != null)
                        {
                            metadataSummary += $"\nType: {document["metadata_content_type"]}";
                        }
                        searchResult.Summary = metadataSummary;
                    }
                    
                    results.Add(searchResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to search documents in Azure AI Search, returning empty results");
                // In case of search error, we'll return an empty list and continue with normal processing
            }
        }

        return results;
    }

    private string BuildEnhancedPrompt(string originalPrompt, List<SearchResult> searchResults)
    {
        if (searchResults == null || !searchResults.Any())
        {
            return originalPrompt; // If no search results, return original prompt
        }

        var context = "Based on the following documents, please answer the question:\n\n";
        
        foreach (var result in searchResults.Take(3)) // Take top 3 results
        {
            context += $"Document: {result.Title}\n";
            context += $"Content: {result.Summary}\n\n";
        }

        context += $"Question: {originalPrompt}\n";
        context += "Answer: ";
        
        return context;
    }



    [HttpGet("documents/{documentId}")]
    public async Task<IActionResult> DownloadDocument(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            return BadRequest(new { error = "Document ID is required" });

        // Check if BlobContainerClient is available
        if (_blobContainerClient == null)
        {
            _logger.LogError("BlobContainerClient is not configured, cannot download file from Azure Blob Storage");
            return StatusCode(500, new { error = "Blob storage is not configured properly" });
        }

        try
        {
            // Get the blob client for the specific document
            var blobClient = _blobContainerClient.GetBlobClient(documentId);

            // Check if the blob exists
            var existsResponse = await blobClient.ExistsAsync();
            if (!existsResponse.Value)
            {
                return NotFound(new { error = "Document not found" });
            }

            // Download the blob content
            var response = await blobClient.DownloadAsync();
            var content = response.Value.Content;
            var contentType = response.Value.ContentType ?? "application/octet-stream";

            // Get the blob properties to determine the original filename
            var propertiesResponse = await blobClient.GetPropertiesAsync();
            var originalFileName = propertiesResponse.Value.Metadata.ContainsKey("originalfilename") 
                ? propertiesResponse.Value.Metadata["originalfilename"] 
                : documentId;

            // Return the file content with appropriate headers
            return File(content, contentType, originalFileName);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Document with ID {DocumentId} not found in Azure Blob Storage", documentId);
            return NotFound(new { error = "Document not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while downloading document {DocumentId} from Azure Blob Storage", documentId);
            return StatusCode(500, new { error = "An error occurred while downloading the document" });
        }
    }

    // Enhanced search combining both keyword and vector search
    private async Task<List<SearchResult>> EnhancedSearchDocumentsAsync(string query)
    {
        var results = new List<SearchResult>();
        
        // Perform both keyword search and vector search, then merge results
        var keywordResults = await FallbackSearchDocumentsAsync(query);
        var vectorResults = await VectorSearchDocumentsAsync(query);
        
        // Combine and deduplicate results
        var allResults = new Dictionary<string, SearchResult>();
        
        // Add keyword results
        foreach (var result in keywordResults)
        {
            if (!string.IsNullOrEmpty(result.DocumentId))
            {
                allResults[result.DocumentId] = result;
            }
            else
            {
                // If no document ID, use the URL as a key
                allResults[result.Url] = result;
            }
        }
        
        // Add vector search results (prefer these if there are duplicates)
        foreach (var result in vectorResults)
        {
            if (!string.IsNullOrEmpty(result.DocumentId))
            {
                allResults[result.DocumentId] = result;
            }
            else
            {
                // If no document ID, use the URL as a key
                allResults[result.Url] = result;
            }
        }
        
        // Sort results by score (higher scores first)
        results = allResults.Values.OrderByDescending(r => r.Score).ToList();
        
        return results;
    }

    // Fallback search using Azure AI Search keyword search via SearchClient
    private async Task<List<SearchResult>> FallbackSearchDocumentsAsync(string query)
    {
        var results = new List<SearchResult>();
        
        // Only perform fallback search if SearchClient is available
        if (_searchClient != null)
        {
            try
            {
                var searchOptions = new SearchOptions
                {
                    Size = 5, // Return top 5 results
                    IncludeTotalCount = true
                };

                var response = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    var document = result.Document;
                    var searchResult = new SearchResult
                    {
                        Title = document.ContainsKey("title") ? document["title"]?.ToString() ?? "" : "",
                        Summary = document.ContainsKey("content") ? document["content"]?.ToString() ?? "" : "",
                        Url = document.ContainsKey("url") ? document["url"]?.ToString() ?? "" : "",
                        Score = result.Score ?? 0
                    };
                    
                    // Extract blob name from metadata_storage_path if available
                    // metadata_storage_path contains the base64-encoded path of the blob
                    if (document.ContainsKey("metadata_storage_path") && document["metadata_storage_path"] != null)
                    {
                        var encodedPath = document["metadata_storage_path"]?.ToString();
                        if (!string.IsNullOrEmpty(encodedPath))
                        {
                            try
                            {
                                // Decode the base64 path to get the blob name
                                var decodedPathBytes = Convert.FromBase64String(encodedPath);
                                var decodedPath = System.Text.Encoding.UTF8.GetString(decodedPathBytes);
                                // The path is in format /container-name/blob-name
                                var pathParts = decodedPath.Split('/');
                                if (pathParts.Length >= 3)
                                {
                                    var blobName = pathParts[2]; // Get the blob name part
                                    searchResult.DocumentId = blobName; // Store the document ID directly
                                    searchResult.Url = $"/documents/{blobName}"; // Custom URL format for our download endpoint
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Could not decode metadata_storage_path: {EncodedPath}", encodedPath);
                            }
                        }
                    }
                    
                    // If DocumentId is still null, try to get it from the search result's ID field as fallback
                    if (string.IsNullOrEmpty(searchResult.DocumentId))
                    {
                        // Check if the document has an 'id' field that can be used as DocumentId
                        if (document.ContainsKey("id") && document["id"] != null)
                        {
                            var docId = document["id"]?.ToString();
                            if (!string.IsNullOrEmpty(docId))
                            {
                                searchResult.DocumentId = docId;
                            }
                        }
                    }
                    
                    // If DocumentId is still null but we have a URL, try to extract from URL
                    if (string.IsNullOrEmpty(searchResult.DocumentId) && !string.IsNullOrEmpty(searchResult.Url))
                    {
                        try
                        {
                            var uri = new Uri(searchResult.Url);
                            var path = uri.AbsolutePath;
                            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            if (pathParts.Length > 0)
                            {
                                searchResult.DocumentId = pathParts[pathParts.Length - 1]; // Last part as fallback
                            }
                        }
                        catch (UriFormatException)
                        {
                            // If URL is malformed, try simple split
                            var parts = searchResult.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0)
                            {
                                searchResult.DocumentId = parts[parts.Length - 1];
                            }
                        }
                    }
                    
                    // If primary fields are empty/meaningless, try to populate from available metadata
                    if (string.IsNullOrWhiteSpace(searchResult.Title))
                    {
                        // Try to extract title from metadata_storage_name
                        if (document.ContainsKey("metadata_storage_name") && document["metadata_storage_name"] != null)
                        {
                            searchResult.Title = document["metadata_storage_name"]?.ToString() ?? "Untitled Document";
                        }
                        else
                        {
                            // Use the document ID as title if available
                            searchResult.Title = string.IsNullOrEmpty(searchResult.DocumentId) ? "Untitled Document" : searchResult.DocumentId;
                        }
                    }
                    
                    // If content is empty/meaningless, provide a meaningful summary from available metadata
                    if (string.IsNullOrWhiteSpace(searchResult.Summary) || searchResult.Summary == "\n\n")
                    {
                        var metadataSummary = $"Document: {searchResult.Title}";
                        if (document.ContainsKey("metadata_storage_last_modified") && document["metadata_storage_last_modified"] != null)
                        {
                            metadataSummary += $"\nModified: {document["metadata_storage_last_modified"]}";
                        }
                        if (document.ContainsKey("metadata_content_type") && document["metadata_content_type"] != null)
                        {
                            metadataSummary += $"\nType: {document["metadata_content_type"]}";
                        }
                        searchResult.Summary = metadataSummary;
                    }
                    
                    results.Add(searchResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to search documents in Azure AI Search, returning empty results");
                // In case of search error, we'll return an empty list and continue with normal processing
            }
        }

        return results;
    }

    private async Task<List<SearchResult>> VectorSearchDocumentsAsync(string query)
    {
        var results = new List<SearchResult>();
        
        // Only proceed if we have both embedding service and indexer service
        if (_embeddingService != null && _indexerService != null)
        {
            try
            {
                // Generate embeddings for the query
                var queryEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(query);
                
                if (queryEmbeddings != null && queryEmbeddings.Length > 0)
                {
                    // Perform vector search
                    var vectorSearchResults = await _indexerService.VectorSearchAsync(query, queryEmbeddings, 5);
                    results.AddRange(vectorSearchResults);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to perform vector search for query: {Query}", query);
                // In case of vector search error, we'll return an empty list and continue with normal processing
            }
        }

        return results;
    }

    [HttpPost("uploadWithEmbeddings")]
    public async Task<IActionResult> UploadFileWithEmbeddings(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "File is required" });

        // Check if file type is allowed
        var allowedExtensions = new[] { ".pdf", ".txt", ".docx", ".doc", ".xls", ".xlsx", ".ppt", ".pptx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { error = $"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" });
        }

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
        {
            return BadRequest(new { error = "File size exceeds 10MB limit" });
        }

        try
        {
            // Check if required services are available
            if (_blobContainerClient == null || _embeddingService == null || _indexerService == null || _documentProcessingService == null)
            {
                _logger.LogError("Required services (BlobContainerClient, EmbeddingService, IndexerService, DocumentProcessingService) are not configured");
                return StatusCode(500, new { error = "Required services are not configured properly" });
            }

            // Process and chunk the document
            var blobName = $"{Guid.NewGuid()}{fileExtension}";
            using var fileStream = file.OpenReadStream();
            await _blobContainerClient.UploadBlobAsync(blobName, fileStream);

            // Process and extract text from the document
            var chunks = await _documentProcessingService.ProcessAndChunkDocumentAsync(blobName, _blobContainerClient);

            // Generate embeddings for each chunk and index them
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var chunkEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(chunk);
                
                // Create a unique ID for this chunk using only valid characters for Azure AI Search
                var chunkId = $"{blobName.Replace(".", "_")}_chunk_{i}";
                
                // Prepare metadata
                var metadata = new Dictionary<string, object>
                {
                    ["uploadedat"] = DateTime.UtcNow.ToString("O"),
                    ["contenttype"] = file.ContentType,
                    ["title"] = $"{Path.GetFileNameWithoutExtension(file.FileName)} - Chunk {i + 1}"
                };

                // Index the document chunk with embeddings
                var indexed = await _indexerService.IndexDocumentWithEmbeddingsAsync(chunkId, chunk, chunkEmbeddings, metadata);
                
                if (!indexed)
                {
                    _logger.LogError("Failed to index chunk {ChunkId} for document {BlobName}", chunkId, blobName);
                }
                else
                {
                    _logger.LogInformation("Successfully indexed chunk {ChunkId} for document {BlobName}", chunkId, blobName);
                }
            }

            // Return success response
            var blobUrl = $"{_blobContainerClient.Uri}/{blobName}";
            return Ok(new { 
                message = $"File uploaded to Azure Blob Storage and indexed with embeddings successfully. {chunks.Count} chunks created.", 
                fileName = file.FileName,
                blobUrl = blobUrl,
                chunksCreated = chunks.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading file {FileName} with embeddings to Azure", file.FileName);
            return StatusCode(500, new { error = "An error occurred while uploading the file with embeddings" });
        }
    }

    // The IndexDocumentAsync method is no longer used since indexing is handled by Azure AI Search indexer
    // The Azure AI Search indexer will automatically process documents in the blob container

    // Text extraction methods are no longer needed since Azure AI Search indexer handles this
}



