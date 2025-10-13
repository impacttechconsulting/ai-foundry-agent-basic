namespace AiFoundryAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(IOpenAIService openAIService, ILogger<AgentController> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    /// <summary>
    /// Process a request with the AI agent
    /// </summary>
    /// <param name="request">The agent request containing the input text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI agent's response</returns>
    [HttpPost("process")]
    public async Task<ActionResult<string>> ProcessAgentRequest([FromBody] AgentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Input))
        {
            return BadRequest("Input is required");
        }

        try
        {
            var result = await _openAIService.ProcessAgentRequestAsync(request.Input, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing agent request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}

/// <summary>
/// Represents a request to the AI agent
/// </summary>
public class AgentRequest
{
    /// <summary>
    /// The input text for the agent
    /// </summary>
    public string Input { get; set; } = string.Empty;
}