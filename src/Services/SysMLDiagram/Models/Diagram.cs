namespace SysArx.Services.SysMLDiagram.Models;

public class DiagramItem
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class Diagram
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DiagramName { get; set; } = string.Empty;
    public List<DiagramItem> Items { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    public Diagram()
    {
    }

    public Diagram(string userId, string diagramName = "Default Diagram")
    {
        Id = Guid.NewGuid().ToString();
        UserId = userId;
        DiagramName = diagramName;
        Items = new List<DiagramItem>();
    }
}
