namespace AiFoundryAgent.Configuration;

public class ChatApiOptions
{
    [Url]
    public string AIProjectEndpoint { get; init; } = default!;

    [Required]
    public string AIAgentId { get; init; } = default!;
}