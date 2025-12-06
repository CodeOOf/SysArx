using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SysArx.Services.SysMLStore.Models;
using SysArx.Services.SysMLStore.Settings;

namespace SysArx.Services.SysMLStore.Storage;

/// <summary>
/// NoSQL storage provider using MongoDB.
/// Default storage backend for SysML models.
/// </summary>
public class NoSqlStorageProvider : IStorageProvider
{
    private readonly IMongoCollection<SysMLItem> _items;
    private readonly ILogger<NoSqlStorageProvider> _logger;

    public string ProviderType => "NoSQL";

    public NoSqlStorageProvider(IOptions<MongoDbSettings> settings, ILogger<NoSqlStorageProvider> logger)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _items = database.GetCollection<SysMLItem>(settings.Value.CollectionName);
        _logger = logger;
        
        _logger.LogInformation("NoSQL Storage Provider initialized with MongoDB");
    }

    public async Task<List<SysMLItem>> GetAllAsync()
    {
        _logger.LogDebug("Retrieving all SysML items from NoSQL storage");
        return await _items.Find(_ => true).ToListAsync();
    }

    public async Task<SysMLItem?> GetByIdAsync(string id)
    {
        _logger.LogDebug("Retrieving SysML item {ItemId} from NoSQL storage", id);
        return await _items.Find(item => item.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<SysMLItem>> GetByTypeAsync(string type)
    {
        _logger.LogDebug("Retrieving SysML items of type {Type} from NoSQL storage", type);
        return await _items.Find(item => item.Type == type).ToListAsync();
    }

    public async Task<SysMLItem> CreateAsync(SysMLItem item)
    {
        item.CreatedDate = DateTime.UtcNow;
        item.ModifiedDate = DateTime.UtcNow;
        await _items.InsertOneAsync(item);
        _logger.LogInformation("Created SysML item {ItemId} in NoSQL storage", item.Id);
        return item;
    }

    public async Task UpdateAsync(string id, SysMLItem item)
    {
        item.ModifiedDate = DateTime.UtcNow;
        await _items.ReplaceOneAsync(x => x.Id == id, item);
        _logger.LogInformation("Updated SysML item {ItemId} in NoSQL storage", id);
    }

    public async Task DeleteAsync(string id)
    {
        await _items.DeleteOneAsync(item => item.Id == id);
        _logger.LogInformation("Deleted SysML item {ItemId} from NoSQL storage", id);
    }
}
