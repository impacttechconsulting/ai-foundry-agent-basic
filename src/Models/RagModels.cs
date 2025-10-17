using System.Text.Json.Serialization;

namespace AiFoundryAgent.Models;

public class RagCompletionRequest
{
    public string? Prompt { get; set; }
    public List<string>? DocumentIds { get; set; } = new List<string>();
}

public class SearchResult
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public double Score { get; set; }
}

// This class is for mapping Azure Search results
public class SearchResultIndex
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}