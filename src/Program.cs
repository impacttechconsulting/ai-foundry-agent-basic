using AiFoundryAgent.Configuration;
using Microsoft.AspNetCore.StaticFiles;

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
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddOptions<ChatApiOptions>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton((provider) =>
{
    var config = provider.GetRequiredService<IOptions<ChatApiOptions>>().Value;
    Console.WriteLine($"AIProjectEndpoint: {config.AIProjectEndpoint}");
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

// app.UseHttpsRedirection();

// Serve static files (CSS, JS, images) from wwwroot
app.UseStaticFiles();

app.UseRouting();

// Catch-all route to serve React app for client-side routing
app.MapFallbackToFile("/{*path:nonfile}", "index.html");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseCors("AllowAllOrigins");
app.MapControllers();
app.Start();

var server = app.Services.GetRequiredService<IServer>();
IServerAddressesFeature? addressFeature = server.Features.Get<IServerAddressesFeature>();
foreach (var address in addressFeature?.Addresses ?? [])
{
    var uri = new Uri(address);
    logger.LogInformation($"Kestrel is listening on address: {address}");
    logger.LogInformation($"Kestrel is listening on port: {uri.Port}");
}
// app.MapGet("/", () => $"Hi there, Kestrel is running on\n\n{string.Join("\n", addressFeature?.Addresses ?? [])}");
app.WaitForShutdown();
