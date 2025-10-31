namespace AiFoundryAgent.Models;

public class AgentListResponse
{
    public List<PersistentAgent> Agents { get; set; } = new();
    public int TotalCount { get; set; }
}

public class AgentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}

public class CreateAgentRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Model { get; set; } = string.Empty;
    
    [Required]
    public string Instructions { get; set; } = string.Empty;
}

public class UpdateAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}

public class ModelDeployment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}