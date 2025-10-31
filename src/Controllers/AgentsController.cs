using Azure.AI.Projects;

namespace AiFoundryAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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

    [HttpGet("models")]
    public async Task<ActionResult<IAsyncEnumerator<AIProjectDeployment>>> GetModelDeploymentsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching list of model deployments from Azure AI Foundry");
            var projectClient = new AIProjectClient(new Uri("https://your-ai-foundry-project.eastus.inference.ai.azure.com"), new DefaultAzureCredential());
            var deployments = projectClient.Deployments.GetDeploymentsAsync();
            // _logger.LogInformation("Successfully returned {Count} model deployments", modelDeployments.Count);
            return Ok(deployments.GetAsyncEnumerator());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching model deployments from Azure AI Foundry");
            return StatusCode(500, new { error = "An error occurred while retrieving the model deployments" });
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

    [HttpPost]
    public async Task<ActionResult<AgentDto>> CreateAgentAsync([FromBody] CreateAgentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Creating new agent: {AgentName}", request.Name);

            // Create the agent using the Azure AI Foundry service
            var response = await _client.Administration.CreateAgentAsync(
                request.Name,
                request.Model,
                request.Instructions,
                request.Description);

            var createdAgent = response.Value;

            var agentDto = new AgentDto
            {
                Id = createdAgent.Id,
                Name = createdAgent.Name,
                Description = createdAgent.Description,
                Model = createdAgent.Model,
                Instructions = createdAgent.Instructions
            };

            _logger.LogInformation("Successfully created agent: {AgentId} - {AgentName}", agentDto.Id, agentDto.Name);

            return CreatedAtAction(nameof(GetAgentAsync), new { id = agentDto.Id }, agentDto);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error occurred while creating agent: {AgentName}", request.Name);
            return StatusCode(500, new { error = $"An error occurred while creating the agent: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating agent: {AgentName}", request.Name);
            return StatusCode(500, new { error = "An error occurred while creating the agent" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AgentDto>> UpdateAgentAsync(string id, [FromBody] UpdateAgentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Updating agent with ID: {AgentId}", id);

            // Get the existing agent first to ensure it exists
            var existingAgent = await GetAgentFromAzureAsync(id);
            if (existingAgent == null)
            {
                _logger.LogWarning("Agent with ID {AgentId} not found for update", id);
                return NotFound(new { error = "Agent not found" });
            }

            // Update the agent in Azure AI Foundry
            var response = await _client.Administration.UpdateAgentAsync(
                id,
                request.Name,
                request.Description,
                request.Instructions);

            var updatedAgent = response.Value;

            var agentDto = new AgentDto
            {
                Id = updatedAgent.Id,
                Name = updatedAgent.Name,
                Description = updatedAgent.Description,
                Model = updatedAgent.Model,
                Instructions = updatedAgent.Instructions
            };

            _logger.LogInformation("Successfully updated agent: {AgentId} - {AgentName}", agentDto.Id, agentDto.Name);

            return Ok(agentDto);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error occurred while updating agent with ID: {AgentId}", id);
            return StatusCode(500, new { error = $"An error occurred while updating the agent: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating agent with ID: {AgentId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the agent" });
        }
    }
}