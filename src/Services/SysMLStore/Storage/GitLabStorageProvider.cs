using System.Text;
using System.Text.Json;
using SysArx.Services.SysMLStore.Models;
using SysArx.Services.SysMLStore.Settings;
using Microsoft.Extensions.Options;

namespace SysArx.Services.SysMLStore.Storage;

/// <summary>
/// GitLab storage provider for SysML models.
/// Stores .sysml files in a GitLab repository.
/// </summary>
public class GitLabStorageProvider : IStorageProvider
{
    private readonly GitStorageSettings _settings;
    private readonly ILogger<GitLabStorageProvider> _logger;
    private readonly HttpClient _httpClient;

    public string ProviderType => "GitLab";

    public GitLabStorageProvider(
        IOptions<StorageSettings> storageSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<GitLabStorageProvider> logger)
    {
        _settings = storageSettings.Value.GitLab ?? throw new ArgumentException("GitLab settings not configured");
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("GitLab");
        
        var apiEndpoint = _settings.ApiEndpoint ?? "https://gitlab.com/api/v4";
        _httpClient.BaseAddress = new Uri(apiEndpoint);
        _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", _settings.Token);

        _logger.LogInformation("GitLab Storage Provider initialized for {Owner}/{Repo}", _settings.Owner, _settings.Repository);
    }

    private string GetProjectId() => Uri.EscapeDataString($"{_settings.Owner}/{_settings.Repository}");

    public async Task<List<SysMLItem>> GetAllAsync()
    {
        _logger.LogDebug("Retrieving all SysML items from GitLab storage");
        
        try
        {
            var projectId = GetProjectId();
            var url = $"/projects/{projectId}/repository/tree?path={_settings.BasePath}&ref={_settings.Branch}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var files = await response.Content.ReadFromJsonAsync<List<GitLabFileInfo>>();
            if (files == null) return new List<SysMLItem>();

            var items = new List<SysMLItem>();
            foreach (var file in files.Where(f => f.Name.EndsWith(".sysml") && f.Type == "blob"))
            {
                var item = await GetByIdAsync(Path.GetFileNameWithoutExtension(file.Name));
                if (item != null) items.Add(item);
            }

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all items from GitLab storage");
            throw;
        }
    }

    public async Task<SysMLItem?> GetByIdAsync(string id)
    {
        _logger.LogDebug("Retrieving SysML item {ItemId} from GitLab storage", id);
        
        try
        {
            var projectId = GetProjectId();
            var filePath = Uri.EscapeDataString($"{_settings.BasePath}/{id}.sysml");
            var url = $"/projects/{projectId}/repository/files/{filePath}?ref={_settings.Branch}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Item {ItemId} not found in GitLab storage", id);
                return null;
            }

            var fileInfo = await response.Content.ReadFromJsonAsync<GitLabFileContent>();
            if (fileInfo?.Content == null) return null;

            var content = Encoding.UTF8.GetString(Convert.FromBase64String(fileInfo.Content));
            return JsonSerializer.Deserialize<SysMLItem>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {ItemId} from GitLab storage", id);
            return null;
        }
    }

    public async Task<List<SysMLItem>> GetByTypeAsync(string type)
    {
        _logger.LogDebug("Retrieving SysML items of type {Type} from GitLab storage", type);
        
        var allItems = await GetAllAsync();
        return allItems.Where(item => item.Type == type).ToList();
    }

    public async Task<SysMLItem> CreateAsync(SysMLItem item)
    {
        item.CreatedDate = DateTime.UtcNow;
        item.ModifiedDate = DateTime.UtcNow;

        var projectId = GetProjectId();
        var filePath = Uri.EscapeDataString($"{_settings.BasePath}/{item.Id}.sysml");
        var content = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });

        var payload = new
        {
            branch = _settings.Branch,
            content,
            commit_message = $"Create SysML item {item.Id}"
        };

        var url = $"/projects/{projectId}/repository/files/{filePath}";
        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Created SysML item {ItemId} in GitLab storage", item.Id);
        return item;
    }

    public async Task UpdateAsync(string id, SysMLItem item)
    {
        item.ModifiedDate = DateTime.UtcNow;

        var projectId = GetProjectId();
        var filePath = Uri.EscapeDataString($"{_settings.BasePath}/{id}.sysml");
        var content = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });

        var payload = new
        {
            branch = _settings.Branch,
            content,
            commit_message = $"Update SysML item {id}"
        };

        var url = $"/projects/{projectId}/repository/files/{filePath}";
        var response = await _httpClient.PutAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Updated SysML item {ItemId} in GitLab storage", id);
    }

    public async Task DeleteAsync(string id)
    {
        var projectId = GetProjectId();
        var filePath = Uri.EscapeDataString($"{_settings.BasePath}/{id}.sysml");

        var payload = new
        {
            branch = _settings.Branch,
            commit_message = $"Delete SysML item {id}"
        };

        var url = $"/projects/{projectId}/repository/files/{filePath}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(payload)
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Deleted SysML item {ItemId} from GitLab storage", id);
    }

    private class GitLabFileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    private class GitLabFileContent
    {
        public string Content { get; set; } = string.Empty;
        public string File_path { get; set; } = string.Empty;
    }
}
