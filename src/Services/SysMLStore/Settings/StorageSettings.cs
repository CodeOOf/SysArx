namespace SysArx.Services.SysMLStore.Settings;

/// <summary>
/// Storage configuration settings.
/// Determines which storage backend to use: NoSQL or External Git Providers.
/// </summary>
public class StorageSettings
{
    /// <summary>
    /// Storage provider type: "NoSQL", "GitHub", or "GitLab".
    /// Default: "NoSQL"
    /// </summary>
    public string Provider { get; set; } = "NoSQL";

    /// <summary>
    /// MongoDB settings (used when Provider = "NoSQL").
    /// </summary>
    public MongoDbSettings? MongoDb { get; set; }

    /// <summary>
    /// GitHub settings (used when Provider = "GitHub").
    /// </summary>
    public GitStorageSettings? GitHub { get; set; }

    /// <summary>
    /// GitLab settings (used when Provider = "GitLab").
    /// </summary>
    public GitStorageSettings? GitLab { get; set; }
}

/// <summary>
/// Git-based storage settings for external git providers.
/// </summary>
public class GitStorageSettings
{
    /// <summary>
    /// Personal Access Token for authentication.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Repository owner/organization.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Repository name.
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Branch name for storing models.
    /// Default: "main"
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// Base path within repository for storing .sysml files.
    /// Default: "models"
    /// </summary>
    public string BasePath { get; set; } = "models";

    /// <summary>
    /// API endpoint (for self-hosted GitLab instances).
    /// Default: "https://gitlab.com/api/v4" for GitLab, "https://api.github.com" for GitHub
    /// </summary>
    public string? ApiEndpoint { get; set; }
}
