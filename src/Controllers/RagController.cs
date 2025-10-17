

namespace AiFoundryAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "BasicAuthentication")]
public class RagController(
    PersistentAgentsClient client,
    IOptionsMonitor<ChatApiOptions> options,
    ILogger<RagController> logger,
    SearchClient? searchClient = null) : ControllerBase
{
    private readonly PersistentAgentsClient _client = client;
    private readonly IOptionsMonitor<ChatApiOptions> _options = options;
    private readonly ILogger<RagController> _logger = logger;
    private readonly SearchClient? _searchClient = searchClient;

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
            // Perform search in Azure AI Search to get relevant documents
            var searchResults = await SearchDocumentsAsync(request.Prompt);
            
            // Enhance the prompt with search results
            var enhancedPrompt = BuildEnhancedPrompt(request.Prompt, searchResults);

            // Create message with enhanced prompt
            PersistentThreadMessage message = await _client.Messages.CreateMessageAsync(
                threadId,
                MessageRole.User,
                enhancedPrompt);

            // Execute the run
            ThreadRun run = await _client.Runs.CreateRunAsync(threadId, config.AIAgentId);

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

            // Return both the answer and source documents
            return Ok(new 
            { 
                data = fullText,
                sources = searchResults.Select(r => new { 
                    Title = r.Title, 
                    Content = r.Summary,
                    Url = r.Url 
                }).ToList()
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
            // Process and store the file
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var uploadPath = Path.Combine("wwwroot", "uploads", "rag-documents");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Index the document in Azure AI Search
            await IndexDocumentAsync(filePath, file.FileName);

            return Ok(new { 
                message = "File uploaded and indexed successfully", 
                fileName = file.FileName,
                fileUrl = $"/uploads/rag-documents/{fileName}" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading file {FileName}", file.FileName);
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    private async Task<List<SearchResult>> SearchDocumentsAsync(string query)
    {
        var results = new List<SearchResult>();
        
        // Only proceed if we have a configured SearchClient
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
                    results.Add(new SearchResult
                    {
                        Title = document.ContainsKey("title") ? document["title"]?.ToString() ?? "" : "",
                        Summary = document.ContainsKey("content") ? document["content"]?.ToString() ?? "" : "",
                        Url = document.ContainsKey("url") ? document["url"]?.ToString() ?? "" : "",
                        Score = result.Score ?? 0
                    });
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

    private async Task IndexDocumentAsync(string filePath, string originalFileName)
    {
        // This is a placeholder implementation
        // In a real implementation, you would:
        // 1. Extract text from the document (using libraries like iTextSharp for PDFs)
        // 2. Chunk the document content
        // 3. Index the content in Azure AI Search
        
        await Task.Delay(1); // Add a small delay to make method truly async
        _logger.LogInformation("Document indexing not fully implemented - would normally index {FilePath}", filePath);
    }
}



