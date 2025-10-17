using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AiFoundryAgent.Services;

public interface IIndexerService
{
    Task<bool> RunIndexerAsync(string indexerName);
    Task<bool> CreateOrUpdateDataSourceConnectionAsync(string dataSourceName, string storageConnectionString, string containerName);
    Task<bool> CreateOrUpdateIndexerAsync(string indexerName, string dataSourceName, string indexName);
}

public class IndexerService : IIndexerService
{
    private readonly HttpClient _httpClient;
    private readonly string _searchEndpoint;
    private readonly string _adminKey;
    private readonly ILogger<IndexerService> _logger;

    public IndexerService(IOptionsMonitor<IndexerServiceSearchOptions> options, ILogger<IndexerService> logger)
    {
        var searchOptions = options.CurrentValue;
        _searchEndpoint = searchOptions.Endpoint;
        _adminKey = searchOptions.ApiKey;
        _logger = logger;
        
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

    public async Task<bool> CreateOrUpdateDataSourceConnectionAsync(string dataSourceName, string storageConnectionString, string containerName)
    {
        try
        {
            var dataSourceUri = $"{_searchEndpoint.TrimEnd('/')}/datasources('{dataSourceName}')?api-version=2023-10-01-Preview";
            
            var dataSource = new
            {
                name = dataSourceName,
                type = "azureblob",
                connectionString = storageConnectionString,
                container = new { name = containerName }
            };

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
                schedule = new { interval = "PT5M" }, // Run every 5 minutes
                parameters = new
                {
                    configuration = new
                    {
                        dataToExtract = "contentAndMetadata",
                        parsingMode = "default",
                        // Add skillset for enhanced cognitive search if needed
                        // skillsetName = "document-processing-skills"
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
}

public class IndexerServiceSearchOptions  // Renamed to avoid conflict
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
}