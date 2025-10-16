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

// Add OpenAPI (Swagger) services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AiFoundryAgent API", Version = "v1" });
    
    // Add custom authentication to Swagger - using Bearer token scheme to match our auth handler
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",  // Changed to bearer to match our auth handler
        BearerFormat = "base64-encoded-credentials",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the format 'admin|expiry_date' encoded in base64. To get a token, call the /auth/login endpoint, provide valid credentials, and use the returned token here."
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting...");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AiFoundryAgent API v1");
    });
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
    
    // Inform about Swagger endpoint in development
    if (app.Environment.IsDevelopment())
    {
        logger.LogInformation($"Swagger UI is available at: {address}/swagger");
    }
}
app.WaitForShutdown();
