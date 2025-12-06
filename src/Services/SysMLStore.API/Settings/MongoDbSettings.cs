namespace SysArx.Services.SysMLStore.API.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "SysMLStoreDb";
    public string CollectionName { get; set; } = "SysMLItems";
}
