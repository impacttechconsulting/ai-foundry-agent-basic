using Azure;

namespace AiFoundryAgent.Services;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingsAsync(string text);
    Task<List<float[]>> GenerateEmbeddingsBatchAsync(List<string> texts);
}