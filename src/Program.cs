using AiFoundryAgent.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure HTTPS for development environment
#if DEBUG
// In development, use HTTPS with dev certificate
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Enable HTTPS with development certificate
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        // Use the default development certificate
        // In production, Azure App Service handles HTTPS termination
    });
});
#endif

builder.Logging.AddConsole(options =>
{
    options.FormatterName = "simple";
});

builder.Services.AddOptions<ChatApiOptions>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton((provider) =>
{
    var config = provider.GetRequiredService<IOptions<ChatApiOptions>>().Value;
    PersistentAgentsClient client = new(config.AIProjectEndpoint, new DefaultAzureCredential());

    return client;
});

builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting...");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable HTTPS redirection in all environments
// In development: Uses dev certificate
// In production: Azure App Service handles HTTPS termination and forwards internally over HTTP
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseCors("AllowAllOrigins");

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    if (app.Environment.IsDevelopment())
    {
        logger.LogInformation("AI Foundry Agent running on https://localhost:5001 | Chat UI: https://localhost:5001 | API: /chat/threads & /chat/completions/{threadId}");
    }
    else
    {
        logger.LogInformation("AI Foundry Agent deployed to Azure App Service | HTTPS enabled by default | Chat UI and API endpoints available");
    }
});

app.Run();