/*
This file documents how the application is configured to use managed identity
for Azure AI Search authentication, which is more secure than using API keys.

The updated application (Program.cs) now supports both managed identity and API key authentication:

```csharp
if (useManagedIdentity)
{
    // Use managed identity for authentication
    var searchClient = new SearchClient(
        new Uri(searchEndpoint),
        searchIndexName,
        new Azure.Identity.DefaultAzureCredential());
    
    return searchClient;
}
else
{
    // Use API key for authentication (fallback)
    var searchApiKey = config["AzureAISearch:ApiKey"];
    var searchClient = new SearchClient(
        new Uri(searchEndpoint),
        searchIndexName,
        new Azure.AzureKeyCredential(searchApiKey ?? ""));
    
    return searchClient;
}
```

The app service is configured with:
- System-assigned managed identity enabled
- Application settings including AzureAISearch__UseManagedIdentity=true
- Appropriate role assignments via the role assignments defined in role-assignments.tf

This allows the application to authenticate to Azure AI Search without requiring API keys
to be stored and transmitted, improving security posture.
*/