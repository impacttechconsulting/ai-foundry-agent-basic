using AiFoundryAgent.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace AiFoundryAgent.Services;

public interface IAzureSearchSetupService
{
    Task<bool> SetupDataSourceAndIndexerAsync();
}

public class AzureSearchSetupService : IAzureSearchSetupService, IHostedService
{
    private readonly IIndexerService _indexerService;
    private readonly IOptionsMonitor<AzureStorageOptions> _storageOptions;
    private readonly IOptionsMonitor<IndexerServiceSearchOptions> _searchOptions;
    private readonly ILogger<AzureSearchSetupService> _logger;
    private readonly string _dataSourceName = "blob-document-data-source";
    private readonly string _indexerName = "blob-document-indexer";

    public AzureSearchSetupService(
        IIndexerService indexerService, 
        IOptionsMonitor<AzureStorageOptions> storageOptions,
        IOptionsMonitor<IndexerServiceSearchOptions> searchOptions,
        ILogger<AzureSearchSetupService> logger)
    {
        _indexerService = indexerService;
        _storageOptions = storageOptions;
        _searchOptions = searchOptions;
        _logger = logger;
    }

    public async Task<bool> SetupDataSourceAndIndexerAsync()
    {
        try
        {
            _logger.LogInformation("Starting Azure Search setup for data source and indexer");

            // Get storage configuration
            var storageOptions = _storageOptions.CurrentValue;
            
            if (string.IsNullOrEmpty(storageOptions.AccountName))
            {
                _logger.LogWarning("Azure Storage AccountName is not configured. Skipping data source and indexer setup.");
                return false;
            }

            var connectionString = storageOptions.ConnectionString;
            var containerName = storageOptions.ContainerName;
            var useManagedIdentity = storageOptions.UseManagedIdentity;

            if (string.IsNullOrEmpty(connectionString) && !useManagedIdentity)
            {
                _logger.LogWarning("Azure Storage ConnectionString is not configured and managed identity is not enabled. Skipping data source and indexer setup.");
                return false;
            }

            // Create or update the data source connection
            _logger.LogInformation("Creating or updating data source: {DataSourceName}", _dataSourceName);
            var dataSourceCreated = await _indexerService.CreateOrUpdateDataSourceConnectionAsync(
                _dataSourceName, 
                connectionString, 
                containerName,
                useManagedIdentity);

            if (!dataSourceCreated)
            {
                _logger.LogError("Failed to create or update data source: {DataSourceName}", _dataSourceName);
                return false;
            }

            // Get the search index name from configuration
            var searchIndexName = _searchOptions.CurrentValue.IndexName;
            
            if (string.IsNullOrEmpty(searchIndexName))
            {
                _logger.LogWarning("Azure AI Search IndexName is not configured. Using default 'documents' index.");
                searchIndexName = "documents"; // Default fallback
            }

            // Create or update the indexer
            _logger.LogInformation("Creating or updating indexer: {IndexerName}", _indexerName);
            var indexerCreated = await _indexerService.CreateOrUpdateIndexerAsync(
                _indexerName, 
                _dataSourceName, 
                searchIndexName);

            if (!indexerCreated)
            {
                _logger.LogError("Failed to create or update indexer: {IndexerName}", _indexerName);
                return false;
            }

            _logger.LogInformation("Azure Search data source and indexer setup completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Azure Search setup");
            return false;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Azure Search setup service");
        await SetupDataSourceAndIndexerAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Azure Search setup service stopped");
        return Task.CompletedTask;
    }
}