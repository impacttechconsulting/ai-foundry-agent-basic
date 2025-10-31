using AiFoundryAgent.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace AiFoundryAgent.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        var azureStorageSection = configuration.GetSection(AzureStorageOptions.SectionName);
        if (azureStorageSection.Exists())
        {
            services.AddOptions<AzureStorageOptions>()
                .Bind(azureStorageSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<BlobServiceClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
                
                if (!string.IsNullOrEmpty(options.AccountName))
                {
                    if (options.UseManagedIdentity)
                    {
                        // Use managed identity for authentication
                        var blobServiceClient = new BlobServiceClient(
                            new Uri($"https://{options.AccountName}.blob.core.windows.net"),
                            new Azure.Identity.DefaultAzureCredential());
                        
                        return blobServiceClient;
                    }
                    else
                    {
                        // Use connection string for authentication (fallback)
                        if (!string.IsNullOrEmpty(options.ConnectionString))
                        {
                            var blobServiceClient = new BlobServiceClient(options.ConnectionString);
                            return blobServiceClient;
                        }
                        else
                        {
                            // Fallback to account key method if connection string is not provided
                            var blobServiceClient = new BlobServiceClient(
                                new Uri($"https://{options.AccountName}.blob.core.windows.net"),
                                new Azure.Identity.DefaultAzureCredential());
                            
                            return blobServiceClient;
                        }
                    }
                }
                
                // If configuration is not available, create a placeholder client (will not be used)
                return new BlobServiceClient(new Uri("https://placeholder.blob.core.windows.net"), new Azure.Identity.DefaultAzureCredential());
            });
            
            // Also register the container client for the documents container
            services.AddSingleton<BlobContainerClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
                
                if (!string.IsNullOrEmpty(options.AccountName))
                {
                    if (options.UseManagedIdentity)
                    {
                        // Use managed identity for authentication
                        var containerClient = new BlobContainerClient(
                            new Uri($"https://{options.AccountName}.blob.core.windows.net/{options.ContainerName}"),
                            new Azure.Identity.DefaultAzureCredential());
                        
                        return containerClient;
                    }
                    else
                    {
                        // Use connection string for authentication (fallback)
                        if (!string.IsNullOrEmpty(options.ConnectionString))
                        {
                            var containerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);
                            return containerClient;
                        }
                        else
                        {
                            // Fallback to account key method if connection string is not provided
                            var containerClient = new BlobContainerClient(
                                new Uri($"https://{options.AccountName}.blob.core.windows.net/{options.ContainerName}"),
                                new Azure.Identity.DefaultAzureCredential());
                            
                            return containerClient;
                        }
                    }
                }
                
                // If configuration is not available, create a placeholder client (will not be used)
                return new BlobContainerClient(new Uri($"https://placeholder.blob.core.windows.net/documents"), new Azure.Identity.DefaultAzureCredential());
            });
        }

        return services;
    }

    public static IServiceCollection AddEmbeddingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var embeddingSection = configuration.GetSection(EmbeddingServiceOptions.SectionName);
        if (embeddingSection.Exists())
        {
            services.AddOptions<EmbeddingServiceOptions>()
                .Bind(embeddingSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IEmbeddingService, EmbeddingService>();
        }

        return services;
    }

    public static IServiceCollection AddDocumentProcessingServices(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentProcessingService, DocumentProcessingService>();
        return services;
    }
}