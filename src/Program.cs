using AiFoundryAgent.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
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

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options =>
    {
        // Configure using TimeProvider to avoid ISystemClock deprecation
        options.TimeProvider = TimeProvider.System;
    });

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting...");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Catch-all route to serve React app for client-side routing
// This should come after API routes to avoid intercepting them
app.MapFallbackToFile("/{*path:nonfile}", "index.html");

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
app.WaitForShutdown();
