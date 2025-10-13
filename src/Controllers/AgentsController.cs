namespace AiFoundryAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly PersistentAgentsClient _client;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(PersistentAgentsClient client, ILogger<AgentsController> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Get a list of all available agents
    /// </summary>
    /// <returns>A list of agents with their information</returns>
    [HttpGet]
    public async Task<ActionResult<AgentListResponse>> GetAgentsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching list of agents");
            
            // Note: The Azure AI Agents Persistent SDK doesn't have a direct method to list all agents
            // In a real implementation, you would query your agent management system
            // For now, we'll simulate the call that would interact with Azure AI services
            var agents = await GetAgentsFromAzureAsync();

            var response = new AgentListResponse
            {
                Agents = agents,
                TotalCount = agents.Count
            };

            _logger.LogInformation("Successfully retrieved {Count} agents", agents.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching agents list");
            return StatusCode(500, new { error = "An error occurred while retrieving the agents list" });
        }
    }

    private async Task<List<AgentInfo>> GetAgentsFromAzureAsync()
    {
        // Simulate an async call to Azure AI services
        // In a real implementation, you would use the actual SDK to fetch agents
        await Task.Delay(100); // Simulate network delay
        
        return new List<AgentInfo>
        {
            new AgentInfo
            {
                Id = "asst_OHueTDrq9jpp37QlgtXHPzwk",
                Name = "Customer Support Agent",
                Description = "Handles customer support inquiries and resolves common issues",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                Status = "Active",
                Model = "gpt-4"
            },
            new AgentInfo
            {
                Id = "asst_2ndAgent1234567890",
                Name = "Technical Support Agent",
                Description = "Assists with technical problems and troubleshooting",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Status = "Active",
                Model = "gpt-4"
            },
            new AgentInfo
            {
                Id = "asst_3rdAgent0987654321",
                Name = "Sales Assistant",
                Description = "Helps customers with product information and purchase decisions",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Status = "Active",
                Model = "gpt-4"
            }
        };
    }

    /// <summary>
    /// Get a specific agent by ID
    /// </summary>
    /// <param name="id">The agent ID</param>
    /// <returns>Agent information or 404 if not found</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AgentInfo>> GetAgentAsync(string id)
    {
        try
        {
            _logger.LogInformation("Fetching agent with ID: {AgentId}", id);
            
            // In a real implementation, you would fetch the specific agent from the Azure service
            // For now, return mock data if the ID matches one of our known agents
            var agent = await GetAgentFromAzureAsync(id);
            if (agent == null)
            {
                _logger.LogWarning("Agent with ID {AgentId} not found", id);
                return NotFound(new { error = "Agent not found" });
            }

            _logger.LogInformation("Successfully retrieved agent: {AgentName}", agent.Name);
            return Ok(agent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching agent with ID: {AgentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the agent" });
        }
    }

    private async Task<AgentInfo?> GetAgentFromAzureAsync(string agentId)
    {
        // Simulate an async call to Azure AI services
        // In a real implementation, you would use the actual SDK to fetch a specific agent
        await Task.Delay(50); // Simulate network delay
        
        var agents = new List<AgentInfo>
        {
            new AgentInfo
            {
                Id = "asst_OHueTDrq9jpp37QlgtXHPzwk",
                Name = "Customer Support Agent",
                Description = "Handles customer support inquiries and resolves common issues",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                Status = "Active",
                Model = "gpt-4"
            },
            new AgentInfo
            {
                Id = "asst_2ndAgent1234567890",
                Name = "Technical Support Agent",
                Description = "Assists with technical problems and troubleshooting",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Status = "Active",
                Model = "gpt-4"
            }
        };

        return agents.FirstOrDefault(a => a.Id.Equals(agentId, StringComparison.OrdinalIgnoreCase));
    }
}