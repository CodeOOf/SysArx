using SysArx.Services.SysMLStore.Models;
using SysArx.Services.SysMLStore.Storage;

namespace SysArx.Services.SysMLStore.Services;

public interface ISysMLStoreService
{
    Task<List<SysMLItem>> GetAllAsync();
    Task<SysMLItem?> GetByIdAsync(string id);
    Task<List<SysMLItem>> GetByTypeAsync(string type);
    Task<SysMLItem> CreateAsync(SysMLItem item);
    Task UpdateAsync(string id, SysMLItem item);
    Task DeleteAsync(string id);
    string GetStorageProvider();
}

/// <summary>
/// SysML Store Service with pluggable storage backends.
/// Delegates storage operations to the configured IStorageProvider.
/// </summary>
public class SysMLStoreService : ISysMLStoreService
{
    private readonly IStorageProvider _storageProvider;
    private readonly ILogger<SysMLStoreService> _logger;

    public SysMLStoreService(IStorageProvider storageProvider, ILogger<SysMLStoreService> logger)
    {
        _storageProvider = storageProvider;
        _logger = logger;
        _logger.LogInformation("SysMLStoreService initialized with {Provider} storage provider", _storageProvider.ProviderType);
    }

    public async Task<List<SysMLItem>> GetAllAsync()
    {
        return await _storageProvider.GetAllAsync();
    }

    public async Task<SysMLItem?> GetByIdAsync(string id)
    {
        return await _storageProvider.GetByIdAsync(id);
    }

    public async Task<List<SysMLItem>> GetByTypeAsync(string type)
    {
        return await _storageProvider.GetByTypeAsync(type);
    }

    public async Task<SysMLItem> CreateAsync(SysMLItem item)
    {
        return await _storageProvider.CreateAsync(item);
    }

    public async Task UpdateAsync(string id, SysMLItem item)
    {
        await _storageProvider.UpdateAsync(id, item);
    }

    public async Task DeleteAsync(string id)
    {
        await _storageProvider.DeleteAsync(id);
    }

    public string GetStorageProvider()
    {
        return _storageProvider.ProviderType;
    }
}
