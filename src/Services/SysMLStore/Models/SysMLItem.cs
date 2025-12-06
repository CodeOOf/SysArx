using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SysArx.Services.SysMLStore.Models;

public class SysMLItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [BsonElement("modifiedDate")]
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    [BsonElement("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;
}
