using System.ComponentModel.DataAnnotations;

namespace AiFoundryAgent.Configuration;

public class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    [Required]
    public string AccountName { get; set; } = string.Empty;

    public string? ConnectionString { get; set; }

    public string ContainerName { get; set; } = "documents";

    public bool UseManagedIdentity { get; set; } = false;
}