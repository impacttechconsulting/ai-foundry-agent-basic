using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AiFoundryAgent.Tests;

[TestClass]
public class AgentServiceTests
{
    private ServiceProvider? _serviceProvider;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        // Mock configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AZURE_OPENAI_ENDPOINT", "https://test.openai.azure.com/"},
                {"AZURE_OPENAI_API_KEY", "test-key"},
                {"AZURE_OPENAI_DEPLOYMENT_NAME", "test-deployment"}
            })
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddScoped<IOpenAIService, OpenAIService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public void ServiceCollection_Registers_OpenAIService()
    {
        // Arrange & Act
        var service = _serviceProvider!.GetService<IOpenAIService>();
        
        // Assert
        Assert.IsNotNull(service);
        Assert.IsInstanceOfType(service, typeof(OpenAIService));
    }
}