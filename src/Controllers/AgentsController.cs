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

    [HttpGet]
    public async Task<ActionResult<AgentListResponse>> GetAgentsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching list of agents from Azure Foundry services");
            
            // Fetch agents from Azure Foundry services using the PersistentAgentsClient
            var agents = await GetAgentsFromAzureAsync();

            var response = new AgentListResponse
            {
                Agents = agents,
                TotalCount = agents.Count
            };

            _logger.LogInformation("Successfully retrieved {Count} agents from Azure", agents.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching agents list from Azure Foundry services");
            return StatusCode(500, new { error = "An error occurred while retrieving the agents list" });
        }
    }

    private async Task<List<PersistentAgent>> GetAgentsFromAzureAsync()
    {
        var agents = new List<PersistentAgent>();

        try
        {
            // Fetch agents from Azure Foundry services using the Administration API
            await foreach (var agent in _client.Administration.GetAgentsAsync())
            {
                agents.Add(agent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching agents from Azure Foundry services");
            // Return an empty list if there's an error, instead of throwing
            // In a real implementation, you might want more sophisticated error handling
        }

        return agents;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PersistentAgent>> GetAgentAsync(string id)
    {
        try
        {
            _logger.LogInformation("Fetching agent with ID: {AgentId} from Azure Foundry services", id);
            
            // Fetch the specific agent from Azure Foundry services
            var agent = await GetAgentFromAzureAsync(id);
            if (agent == null)
            {
                _logger.LogWarning("Agent with ID {AgentId} not found in Azure services", id);
                return NotFound(new { error = "Agent not found" });
            }

            _logger.LogInformation("Successfully retrieved agent: {AgentName} from Azure", agent.Name ?? "Unnamed Agent");
            return Ok(agent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching agent with ID: {AgentId} from Azure Foundry services", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the agent" });
        }
    }

    private async Task<PersistentAgent?> GetAgentFromAzureAsync(string agentId)
    {
        try
        {
            // Fetch the specific agent from Azure Foundry services using the Administration API
            var response = await _client.Administration.GetAgentAsync(agentId);
            
            if (response.HasValue)
            {
                return response.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching agent {AgentId} from Azure Foundry services", agentId);
            // Return null if there's an error, which will result in a 404
        }

        return null;
    }
}