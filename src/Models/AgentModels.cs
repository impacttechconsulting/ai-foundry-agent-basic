namespace AiFoundryAgent.Models;

public class AgentListResponse
{
    public List<PersistentAgent> Agents { get; set; } = new();
    public int TotalCount { get; set; }
}