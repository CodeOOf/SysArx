using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SysArx.Services.SysMLStore.API.Models;
using SysArx.Services.SysMLStore.API.Settings;

namespace SysArx.Services.SysMLStore.API.Services;

public interface ISysMLStoreService
{
    Task<List<SysMLItem>> GetAllAsync();
    Task<SysMLItem?> GetByIdAsync(string id);
    Task<List<SysMLItem>> GetByTypeAsync(string type);
    Task<SysMLItem> CreateAsync(SysMLItem item);
    Task UpdateAsync(string id, SysMLItem item);
    Task DeleteAsync(string id);
}

public class SysMLStoreService : ISysMLStoreService
{
    private readonly IMongoCollection<SysMLItem> _items;
    private readonly ILogger<SysMLStoreService> _logger;

    public SysMLStoreService(IOptions<MongoDbSettings> settings, ILogger<SysMLStoreService> logger)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _items = database.GetCollection<SysMLItem>(settings.Value.CollectionName);
        _logger = logger;
    }

    public async Task<List<SysMLItem>> GetAllAsync()
    {
        return await _items.Find(_ => true).ToListAsync();
    }

    public async Task<SysMLItem?> GetByIdAsync(string id)
    {
        return await _items.Find(item => item.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<SysMLItem>> GetByTypeAsync(string type)
    {
        return await _items.Find(item => item.Type == type).ToListAsync();
    }

    public async Task<SysMLItem> CreateAsync(SysMLItem item)
    {
        item.CreatedDate = DateTime.UtcNow;
        item.ModifiedDate = DateTime.UtcNow;
        await _items.InsertOneAsync(item);
        _logger.LogInformation("Created SysML item with ID: {ItemId}", item.Id);
        return item;
    }

    public async Task UpdateAsync(string id, SysMLItem item)
    {
        item.ModifiedDate = DateTime.UtcNow;
        await _items.ReplaceOneAsync(x => x.Id == id, item);
        _logger.LogInformation("Updated SysML item with ID: {ItemId}", id);
    }

    public async Task DeleteAsync(string id)
    {
        await _items.DeleteOneAsync(item => item.Id == id);
        _logger.LogInformation("Deleted SysML item with ID: {ItemId}", id);
    }
}
