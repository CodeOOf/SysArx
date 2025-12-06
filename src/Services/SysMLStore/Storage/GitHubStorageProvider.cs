using System.Text;
using System.Text.Json;
using SysArx.Services.SysMLStore.Models;
using SysArx.Services.SysMLStore.Settings;
using Microsoft.Extensions.Options;

namespace SysArx.Services.SysMLStore.Storage;

/// <summary>
/// External git provider storage implementation for GitHub.
/// Stores .sysml files in a GitHub repository.
/// </summary>
public class GitHubStorageProvider : IStorageProvider
{
    private readonly GitStorageSettings _settings;
    private readonly ILogger<GitHubStorageProvider> _logger;
    private readonly HttpClient _httpClient;

    public string ProviderType => "GitHub";

    public GitHubStorageProvider(
        IOptions<StorageSettings> storageSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubStorageProvider> logger)
    {
        _settings = storageSettings.Value.GitHub ?? throw new ArgumentException("GitHub settings not configured");
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("GitHub");
        
        var apiEndpoint = _settings.ApiEndpoint ?? "https://api.github.com";
        _httpClient.BaseAddress = new Uri(apiEndpoint);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_settings.Token}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SysArx-SysMLStore");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

        _logger.LogInformation("GitHub Storage Provider initialized for {Owner}/{Repo}", _settings.Owner, _settings.Repository);
    }

    public async Task<List<SysMLItem>> GetAllAsync()
    {
        _logger.LogDebug("Retrieving all SysML items from GitHub storage");
        
        try
        {
            // List all files in the base path
            var url = $"/repos/{_settings.Owner}/{_settings.Repository}/contents/{_settings.BasePath}?ref={_settings.Branch}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var files = await response.Content.ReadFromJsonAsync<List<GitHubFileInfo>>();
            if (files == null) return new List<SysMLItem>();

            var items = new List<SysMLItem>();
            foreach (var file in files.Where(f => f.Name.EndsWith(".sysml")))
            {
                var item = await GetByIdAsync(Path.GetFileNameWithoutExtension(file.Name));
                if (item != null) items.Add(item);
            }

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all items from GitHub storage");
            throw;
        }
    }

    public async Task<SysMLItem?> GetByIdAsync(string id)
    {
        _logger.LogDebug("Retrieving SysML item {ItemId} from GitHub storage", id);
        
        try
        {
            var filePath = $"{_settings.BasePath}/{id}.sysml";
            var url = $"/repos/{_settings.Owner}/{_settings.Repository}/contents/{filePath}?ref={_settings.Branch}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Item {ItemId} not found in GitHub storage", id);
                return null;
            }

            var fileInfo = await response.Content.ReadFromJsonAsync<GitHubFileContent>();
            if (fileInfo?.Content == null) return null;

            var content = Encoding.UTF8.GetString(Convert.FromBase64String(fileInfo.Content.Replace("\n", "")));
            return JsonSerializer.Deserialize<SysMLItem>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {ItemId} from GitHub storage", id);
            return null;
        }
    }

    public async Task<List<SysMLItem>> GetByTypeAsync(string type)
    {
        _logger.LogDebug("Retrieving SysML items of type {Type} from GitHub storage", type);
        
        var allItems = await GetAllAsync();
        return allItems.Where(item => item.Type == type).ToList();
    }

    public async Task<SysMLItem> CreateAsync(SysMLItem item)
    {
        item.CreatedDate = DateTime.UtcNow;
        item.ModifiedDate = DateTime.UtcNow;

        var filePath = $"{_settings.BasePath}/{item.Id}.sysml";
        var content = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });
        var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

        var payload = new
        {
            message = $"Create SysML item {item.Id}",
            content = base64Content,
            branch = _settings.Branch
        };

        var url = $"/repos/{_settings.Owner}/{_settings.Repository}/contents/{filePath}";
        var response = await _httpClient.PutAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Created SysML item {ItemId} in GitHub storage", item.Id);
        return item;
    }

    public async Task UpdateAsync(string id, SysMLItem item)
    {
        item.ModifiedDate = DateTime.UtcNow;

        // Get current file SHA (required for updates)
        var filePath = $"{_settings.BasePath}/{id}.sysml";
        var getUrl = $"/repos/{_settings.Owner}/{_settings.Repository}/contents/{filePath}?ref={_settings.Branch}";
        var getResponse = await _httpClient.GetAsync(getUrl);
        var fileInfo = await getResponse.Content.ReadFromJsonAsync<GitHubFileContent>();

        var content = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });
        var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

        var payload = new
        {
            message = $"Update SysML item {id}",
            content = base64Content,
            sha = fileInfo?.Sha,
            branch = _settings.Branch
        };

        var url = $"/repos/{_settings.Owner}/{_settings.Repository}/contents/{filePath}";
        var response = await _httpClient.PutAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Updated SysML item {ItemId} in GitHub storage", id);
    }

    public async Task DeleteAsync(string id)
    {
        // Get current file SHA (required for deletion)
        var filePath = $"{_settings.BasePath}/{id}.sysml";
        var getUrl = $"/repos/{_settings.Owner}/{_settings.Repository}/contents/{filePath}?ref={_settings.Branch}";
        var getResponse = await _httpClient.GetAsync(getUrl);
        var fileInfo = await getResponse.Content.ReadFromJsonAsync<GitHubFileContent>();

        var payload = new
        {
            message = $"Delete SysML item {id}",
            sha = fileInfo?.Sha,
            branch = _settings.Branch
        };

        var url = $"/repos/{_settings.Owner}/{_settings.Repository}/contents/{filePath}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(payload)
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Deleted SysML item {ItemId} from GitHub storage", id);
    }

    private class GitHubFileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Sha { get; set; } = string.Empty;
    }

    private class GitHubFileContent
    {
        public string Content { get; set; } = string.Empty;
        public string Sha { get; set; } = string.Empty;
    }
}
