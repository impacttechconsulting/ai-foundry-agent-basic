namespace AiFoundryAgent.Models;

public class AgentInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Active";
    public string Model { get; set; } = string.Empty;
}

public class AgentListResponse
{
    public List<AgentInfo> Agents { get; set; } = new();
    public int TotalCount { get; set; }
}