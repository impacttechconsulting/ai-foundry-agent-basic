using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AI services
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public interface IOpenAIService
{
    Task<string> ProcessAgentRequestAsync(string input, CancellationToken cancellationToken = default);
}

public class OpenAIService : IOpenAIService
{
    private readonly ILogger<OpenAIService> _logger;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _deploymentName;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _endpoint = configuration["AZURE_OPENAI_ENDPOINT"] ?? string.Empty;
        _apiKey = configuration["AZURE_OPENAI_API_KEY"] ?? string.Empty;
        _deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? "gpt-35-turbo";
        _logger = logger;

        if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Azure OpenAI configuration is not set. Using mock responses for demonstration.");
        }
    }

    public async Task<string> ProcessAgentRequestAsync(string input, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would connect to Azure OpenAI
        // For now, using mock response for build verification
        if (!string.IsNullOrEmpty(_endpoint) && !string.IsNullOrEmpty(_apiKey))
        {
            // The real implementation would go here using Azure SDK
            // This is a simplified representation
            _logger.LogInformation("Processing request with Azure OpenAI: {Input}", input);
            return await Task.FromResult($"Mock response for: {input}");
        }
        else
        {
            // Mock response for demonstration purposes
            _logger.LogInformation("Processing request in mock mode: {Input}", input);
            return await Task.FromResult($"AI Agent processed: {input} (using mock service since Azure credentials not configured)");
        }
    }
}