using System.Text;
using System.Text.Json;

namespace AiFoundryAgent.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _deploymentName;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(IOptionsMonitor<EmbeddingServiceOptions> options, ILogger<EmbeddingService> logger)
    {
        var embeddingOptions = options.CurrentValue;
        _deploymentName = embeddingOptions.DeploymentName;
        _endpoint = embeddingOptions.Endpoint;
        _apiKey = embeddingOptions.ApiKey;
        
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingsAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            _logger.LogWarning("Empty text provided for embedding generation");
            return [];
        }

        try
        {
            // Prepare request body for Azure OpenAI embeddings API
            var requestBody = new
            {
                input = text,
                model = _deploymentName  // In Azure, the model is specified as deployment name in the URL
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send request to Azure OpenAI embeddings endpoint
            var requestUri = $"{_endpoint.TrimEnd('/')}/openai/deployments/{_deploymentName}/embeddings?api-version=2023-05-15";
            var response = await _httpClient.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                
                if (doc.RootElement.TryGetProperty("data", out var dataElement) && 
                    dataElement.ValueKind == JsonValueKind.Array && 
                    dataElement.GetArrayLength() > 0)
                {
                    var embeddingElement = dataElement[0];
                    if (embeddingElement.TryGetProperty("embedding", out var embeddingArray))
                    {
                        var embeddingList = new List<float>();
                        foreach (var item in embeddingArray.EnumerateArray())
                        {
                            embeddingList.Add(item.GetSingle());
                        }
                        return [.. embeddingList];
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure OpenAI request failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }
            
            _logger.LogWarning("No embeddings returned for text: {Text}", text);
            return Array.Empty<float>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating embeddings for text: {Text}", text);
            throw;
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsBatchAsync(List<string> texts)
    {
        if (texts == null || texts.Count == 0)
        {
            _logger.LogWarning("Empty texts list provided for batch embedding generation");
            return [];
        }

        try
        {
            // Prepare request body for Azure OpenAI embeddings API
            var requestBody = new
            {
                input = texts,
                model = _deploymentName  // In Azure, the model is specified as deployment name in the URL
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send request to Azure OpenAI embeddings endpoint
            var requestUri = $"{_endpoint.TrimEnd('/')}/openai/deployments/{_deploymentName}/embeddings?api-version=2023-05-15";
            var response = await _httpClient.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                
                var embeddings = new List<float[]>();
                
                if (doc.RootElement.TryGetProperty("data", out var dataElement) && 
                    dataElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var itemElement in dataElement.EnumerateArray())
                    {
                        if (itemElement.TryGetProperty("embedding", out var embeddingArray))
                        {
                            var embeddingList = new List<float>();
                            foreach (var valueElement in embeddingArray.EnumerateArray())
                            {
                                embeddingList.Add(valueElement.GetSingle());
                            }
                            embeddings.Add(embeddingList.ToArray());
                        }
                    }
                }
                
                return embeddings;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure OpenAI request failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }
            
            _logger.LogWarning("No embeddings returned for batch of {Count} texts", texts.Count);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating batch embeddings for {Count} texts", texts.Count);
            throw;
        }
    }
}

public class EmbeddingServiceOptions
{
    public const string SectionName = "AzureOpenAIEmbeddings";

    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "text-embedding-ada-002";  // Default Azure OpenAI embedding model
}