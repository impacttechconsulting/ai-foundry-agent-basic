using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AiFoundryAgent.Services;

public interface IIndexerService
{
    Task<bool> RunIndexerAsync(string indexerName);
    Task<bool> CreateOrUpdateDataSourceConnectionAsync(string dataSourceName, string storageConnectionString, string containerName, bool useManagedIdentity = false);
    Task<bool> CreateOrUpdateIndexerAsync(string indexerName, string dataSourceName, string indexName);
    Task<bool> CheckIndexerExistsAsync(string indexerName);
    Task<bool> CheckDataSourceExistsAsync(string dataSourceName);
    Task<bool> CheckIndexExistsAsync(string indexName);
    Task<bool> CreateOrUpdateIndexAsync(string indexName);
    Task<IEnumerable<SearchResult>> VectorSearchAsync(string query, float[] vector, int top = 5);
    Task<bool> IndexDocumentWithEmbeddingsAsync(string documentId, string content, float[] embeddings, Dictionary<string, object>? metadata = null);
}

public class IndexerService : IIndexerService
{
    private readonly HttpClient _httpClient;
    private readonly string _searchEndpoint;
    private readonly string _adminKey;
    private readonly ILogger<IndexerService> _logger;
    private readonly IOptionsMonitor<IndexerServiceSearchOptions> _searchOptions;

    public IndexerService(IOptionsMonitor<IndexerServiceSearchOptions> options, ILogger<IndexerService> logger)
    {
        var searchOptions = options.CurrentValue;
        _searchEndpoint = searchOptions.Endpoint;
        _adminKey = searchOptions.ApiKey;
        _logger = logger;
        _searchOptions = options;
        
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("api-key", _adminKey);
    }

    public async Task<bool> RunIndexerAsync(string indexerName)
    {
        try
        {
            var requestUri = $"{_searchEndpoint.TrimEnd('/')}/indexers('{indexerName}')/run?api-version=2023-10-01-Preview";
            
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully triggered indexer {IndexerName}", indexerName);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to trigger indexer {IndexerName}. Status: {StatusCode}, Error: {Error}", 
                    indexerName, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while triggering indexer {IndexerName}", indexerName);
            return false;
        }
    }

    public async Task<bool> CreateOrUpdateDataSourceConnectionAsync(string dataSourceName, string storageConnectionString, string containerName, bool useManagedIdentity = false)
    {
        try
        {
            var dataSourceUri = $"{_searchEndpoint.TrimEnd('/')}/datasources('{dataSourceName}')?api-version=2023-10-01-Preview";
            
            object dataSource;
            if (useManagedIdentity)
            {
                // For managed identity, we use a different approach
                // The @odata.type syntax needs to be handled differently in C# anonymous types
                var dataSourceDict = new Dictionary<string, object>
                {
                    ["name"] = dataSourceName,
                    ["type"] = "azureblob",
                    ["credentials"] = new { connectionString = "" }, // Empty for managed identity
                    ["container"] = new { name = containerName },
                    ["@odata.type"] = "#Microsoft.Azure.Search.DataStoreIdentity.UseManagedIdentity"
                };
                dataSource = dataSourceDict;
            }
            else
            {
                // Use connection string approach - credentials need to be nested properly
                dataSource = new
                {
                    name = dataSourceName,
                    type = "azureblob",
                    credentials = new { connectionString = storageConnectionString },
                    container = new { name = containerName }
                };
            }

            var jsonContent = JsonSerializer.Serialize(dataSource, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, dataSourceUri)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                _logger.LogInformation("Successfully created or updated data source {DataSourceName}", dataSourceName);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create or update data source {DataSourceName}. Status: {StatusCode}, Error: {Error}", 
                    dataSourceName, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating or updating data source {DataSourceName}", dataSourceName);
            return false;
        }
    }

    public async Task<bool> CreateOrUpdateIndexerAsync(string indexerName, string dataSourceName, string indexName)
    {
        try
        {
            var indexerUri = $"{_searchEndpoint.TrimEnd('/')}/indexers('{indexerName}')?api-version=2023-10-01-Preview";
            
            var indexer = new
            {
                name = indexerName,
                dataSourceName = dataSourceName,
                targetIndexName = indexName,
                // No schedule - indexer will be run manually after document uploads
                parameters = new
                {
                    configuration = new
                    {
                        dataToExtract = "contentAndMetadata",
                        parsingMode = "default",
                        // Include cognitive search to enhance document processing
                        // Add skillset for enhanced cognitive search if needed
                        // skillsetName = "document-processing-skills"
                    }
                },
                // Define field mappings to populate fields from metadata 
                fieldMappings = new[] {
                    new { 
                        sourceFieldName = "metadata_storage_path", 
                        targetFieldName = "metadata_storage_path" 
                    },
                    new { 
                        sourceFieldName = "metadata_storage_name", 
                        targetFieldName = "metadata_storage_name" 
                    },
                    new { 
                        sourceFieldName = "metadata_storage_last_modified", 
                        targetFieldName = "metadata_storage_last_modified" 
                    },
                    new { 
                        sourceFieldName = "metadata_content_type", 
                        targetFieldName = "metadata_content_type" 
                    },
                    new { 
                        sourceFieldName = "metadata_language", 
                        targetFieldName = "metadata_language" 
                    },
                    // Map metadata fields to our custom fields
                    new { 
                        sourceFieldName = "metadata_storage_name", 
                        targetFieldName = "title"  // Use filename as title if no better title is available
                    },
                    new { 
                        sourceFieldName = "metadata_storage_path", 
                        targetFieldName = "url"  // Use storage path as URL
                    },
                    new { 
                        sourceFieldName = "metadata_storage_path", 
                        targetFieldName = "filepath"  // Use storage path as filepath
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(indexer, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, indexerUri)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                _logger.LogInformation("Successfully created or updated indexer {IndexerName}", indexerName);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create or update indexer {IndexerName}. Status: {StatusCode}, Error: {Error}", 
                    indexerName, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating or updating indexer {IndexerName}", indexerName);
            return false;
        }
    }

    public async Task<bool> CheckIndexerExistsAsync(string indexerName)
    {
        try
        {
            var requestUri = $"{_searchEndpoint.TrimEnd('/')}/indexers('{indexerName}')?api-version=2023-10-01-Preview";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await _httpClient.SendAsync(request);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if indexer {IndexerName} exists", indexerName);
            return false;
        }
    }

    public async Task<bool> CheckDataSourceExistsAsync(string dataSourceName)
    {
        try
        {
            var requestUri = $"{_searchEndpoint.TrimEnd('/')}/datasources('{dataSourceName}')?api-version=2023-10-01-Preview";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await _httpClient.SendAsync(request);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if data source {DataSourceName} exists", dataSourceName);
            return false;
        }
    }

    public async Task<bool> CheckIndexExistsAsync(string indexName)
    {
        try
        {
            var requestUri = $"{_searchEndpoint.TrimEnd('/')}/indexes('{indexName}')?api-version=2023-10-01-Preview";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await _httpClient.SendAsync(request);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if index {IndexName} exists", indexName);
            return false;
        }
    }

    public async Task<bool> CreateOrUpdateIndexAsync(string indexName)
    {
        try
        {
            var indexUri = $"{_searchEndpoint.TrimEnd('/')}/indexes('{indexName}')?api-version=2023-10-01-Preview";
            
            // Create the index definition using dictionaries to avoid type inference issues
            var indexDefinition = new Dictionary<string, object>
            {
                ["name"] = indexName,
                ["fields"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["name"] = "id",
                        ["type"] = "Edm.String",
                        ["key"] = true,
                        ["searchable"] = false,
                        ["filterable"] = true,
                        ["retrievable"] = true,
                        ["sortable"] = true
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "content",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = true,
                        ["filterable"] = false,
                        ["retrievable"] = true,
                        ["sortable"] = false,
                        ["analyzer"] = "standard.lucene"
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "title",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = true,
                        ["filterable"] = false,
                        ["retrievable"] = true,
                        ["sortable"] = false,
                        ["facetable"] = false
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "url",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = true,
                        ["filterable"] = false,
                        ["retrievable"] = true,
                        ["sortable"] = false
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "filepath",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = true,
                        ["filterable"] = false,
                        ["retrievable"] = true,
                        ["sortable"] = false
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "metadata_storage_path",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = true,
                        ["filterable"] = true,  // Changed to true for better filtering
                        ["retrievable"] = true,
                        ["sortable"] = false
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "metadata_storage_name",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = true,  // Changed to true to make searchable
                        ["filterable"] = true,
                        ["retrievable"] = true,
                        ["sortable"] = true   // Changed to true for sorting
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "metadata_storage_last_modified",
                        ["type"] = "Edm.DateTimeOffset",
                        ["key"] = false,
                        ["searchable"] = false,
                        ["filterable"] = true,
                        ["retrievable"] = true,
                        ["sortable"] = true
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "metadata_content_type",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = true,  // Changed to true
                        ["filterable"] = true,
                        ["retrievable"] = true,
                        ["sortable"] = false
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "metadata_language",
                        ["type"] = "Edm.String",
                        ["key"] = false,
                        ["searchable"] = false,
                        ["filterable"] = true,
                        ["retrievable"] = true,
                        ["sortable"] = false
                    },
                    new Dictionary<string, object>
                    {
                        ["name"] = "contentVector",
                        ["type"] = "Collection(Edm.Single)",
                        ["key"] = false,
                        ["searchable"] = true,
                        ["filterable"] = false,
                        ["retrievable"] = true,
                        ["sortable"] = false,
                        ["dimensions"] = 1536,  // Standard dimension for text-embedding-ada-002
                        ["vectorSearchProfile"] = "vector-profile"
                    }
                },
                ["vectorSearch"] = new Dictionary<string, object>
                {
                    ["algorithms"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["name"] = "vector-algorithm",
                            ["kind"] = "hnsw",
                            ["hnswParameters"] = new Dictionary<string, object>
                            {
                                ["m"] = 4,
                                ["efConstruction"] = 400,
                                ["efSearch"] = 500,
                                ["metric"] = "cosine"
                            }
                        }
                    },
                    ["profiles"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["name"] = "vector-profile",
                            ["algorithm"] = "vector-algorithm"
                        }
                    }
                },
                ["suggesters"] = new object[0], // Empty array to avoid type inference issue
                ["scoringProfiles"] = new object[0], // Empty array to avoid type inference issue
                ["defaultScoringProfile"] = ""
            };

            var jsonContent = JsonSerializer.Serialize(indexDefinition, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, indexUri)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                _logger.LogInformation("Successfully created or updated index {IndexName}", indexName);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create or update index {IndexName}. Status: {StatusCode}, Error: {Error}", 
                    indexName, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating or updating index {IndexName}", indexName);
            return false;
        }
    }

    public async Task<IEnumerable<SearchResult>> VectorSearchAsync(string query, float[] vector, int top = 5)
    {
        var results = new List<SearchResult>();
        
        // For vector search, we need to use the SearchClient which should be available
        // Since we don't have direct access to SearchClient in this service, 
        // we'll need to make the API call directly
        try
        {
            var searchUri = $"{_searchEndpoint.TrimEnd('/')}/indexes('{_searchOptions.CurrentValue.IndexName}')/docs/search?api-version=2023-10-01-Preview";
            
            var searchBody = new
            {
                vectorQueries = new[]
                {
                    new
                    {
                        kind = "vector",
                        vector = vector,
                        k = top,
                        fields = "contentVector"
                    }
                },
                select = "id,title,content,url,metadata_storage_name,metadata_storage_path,metadata_content_type",
                top = top
            };

            var jsonContent = JsonSerializer.Serialize(searchBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, searchUri)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("value", out var valueElement))
                {
                    foreach (var item in valueElement.EnumerateArray())
                    {
                        var searchResult = new SearchResult
                        {
                            Title = item.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? "" : "",
                            Summary = item.TryGetProperty("content", out var contentElement) ? contentElement.GetString() ?? "" : "",
                            Url = item.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? "" : "",
                            DocumentId = item.TryGetProperty("id", out var idElement) ? idElement.GetString() : null,
                            Score = item.TryGetProperty("@search.score", out var scoreElement) ? scoreElement.GetDouble() : 0
                        };

                        // If title is empty, try to get it from metadata_storage_name
                        if (string.IsNullOrWhiteSpace(searchResult.Title) && item.TryGetProperty("metadata_storage_name", out var metaNameElement))
                        {
                            searchResult.Title = metaNameElement.GetString() ?? "Untitled Document";
                        }

                        // If summary is empty, build from available metadata
                        if (string.IsNullOrWhiteSpace(searchResult.Summary))
                        {
                            var metadataSummary = $"Document: {searchResult.Title}";
                            if (item.TryGetProperty("metadata_content_type", out var contentTypeElement))
                            {
                                metadataSummary += $"\nType: {contentTypeElement.GetString()}";
                            }
                            searchResult.Summary = metadataSummary;
                        }

                        results.Add(searchResult);
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Vector search failed. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during vector search for query: {Query}", query);
        }

        return results;
    }

    public async Task<bool> IndexDocumentWithEmbeddingsAsync(string documentId, string content, float[] embeddings, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var indexUri = $"{_searchEndpoint.TrimEnd('/')}/indexes('{_searchOptions.CurrentValue.IndexName}')/docs/index?api-version=2023-10-01-Preview";
            
            // Prepare the document with embeddings
            var document = new Dictionary<string, object>
            {
                ["id"] = documentId,
                ["content"] = content,
                ["contentVector"] = embeddings,
                ["title"] = metadata?.GetValueOrDefault("title")?.ToString() ?? Path.GetFileNameWithoutExtension(documentId),
            };

            // Add only allowed metadata fields that match the index schema
            if (metadata != null)
            {
                // Add only properties that exist in the index schema
                var allowedMetadataKeys = new HashSet<string> 
                { 
                    "title", "url", "filepath", "metadata_storage_path", 
                    "metadata_storage_name", "metadata_storage_last_modified", 
                    "metadata_content_type", "metadata_language"
                };
                
                foreach (var kvp in metadata)
                {
                    if (allowedMetadataKeys.Contains(kvp.Key) && !document.ContainsKey(kvp.Key))
                    {
                        document[kvp.Key] = kvp.Value;
                    }
                }
            }

            var documents = new[] { document };
            var requestBody = new { value = documents };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, indexUri)
            {
                Content = httpContent
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                // Check if indexing was successful
                if (root.TryGetProperty("value", out var valueElement))
                {
                    foreach (var item in valueElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("status", out var statusElement) && !statusElement.GetBoolean())
                        {
                            _logger.LogWarning("Document {DocumentId} indexing failed with error: {ErrorMessage}", 
                                documentId, 
                                item.TryGetProperty("errorMessage", out var errorMsgElement) ? errorMsgElement.GetString() : "Unknown error");
                            return false;
                        }
                    }
                }

                _logger.LogInformation("Successfully indexed document {DocumentId} with embeddings", documentId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to index document {DocumentId} with embeddings. Status: {StatusCode}, Error: {Error}", 
                    documentId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while indexing document {DocumentId} with embeddings", documentId);
            return false;
        }
    }
}

public class IndexerServiceSearchOptions  // Renamed to avoid conflict
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
}
