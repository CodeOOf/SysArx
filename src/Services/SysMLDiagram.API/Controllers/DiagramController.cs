using Microsoft.AspNetCore.Mvc;
using SysArx.Services.SysMLDiagram.API.Models;
using SysArx.Services.SysMLDiagram.API.Services;

namespace SysArx.Services.SysMLDiagram.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagramController : ControllerBase
{
    private readonly IDiagramService _diagramService;
    private readonly ILogger<DiagramController> _logger;

    public DiagramController(IDiagramService diagramService, ILogger<DiagramController> logger)
    {
        _diagramService = diagramService;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<Diagram>> Get(string userId)
    {
        var diagram = await _diagramService.GetDiagramAsync(userId);
        
        if (diagram == null)
        {
            return NotFound();
        }

        return Ok(diagram);
    }

    [HttpPost]
    public async Task<ActionResult<Diagram>> Update([FromBody] Diagram diagram)
    {
        if (string.IsNullOrEmpty(diagram.UserId))
        {
            return BadRequest("UserId is required");
        }

        var updated = await _diagramService.UpdateDiagramAsync(diagram);
        return Ok(updated);
    }

    [HttpPost("{userId}/items")]
    public async Task<ActionResult<Diagram>> AddItem(string userId, [FromBody] DiagramItem item)
    {
        var diagram = await _diagramService.GetDiagramAsync(userId);
        
        if (diagram == null)
        {
            diagram = new Diagram(userId);
        }

        var existingItem = diagram.Items.FirstOrDefault(i => i.ItemId == item.ItemId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
            existingItem.Properties = item.Properties;
        }
        else
        {
            diagram.Items.Add(item);
        }

        var updated = await _diagramService.UpdateDiagramAsync(diagram);
        return Ok(updated);
    }

    [HttpPut("{userId}/items/{itemId}")]
    public async Task<ActionResult<Diagram>> UpdateItem(string userId, string itemId, [FromBody] DiagramItem item)
    {
        var diagram = await _diagramService.GetDiagramAsync(userId);
        
        if (diagram == null)
        {
            return NotFound("Diagram not found");
        }

        var existingItem = diagram.Items.FirstOrDefault(i => i.ItemId == itemId);
        if (existingItem == null)
        {
            return NotFound("Item not found in diagram");
        }

        existingItem.Quantity = item.Quantity;
        existingItem.ItemName = item.ItemName;
        existingItem.ItemType = item.ItemType;
        existingItem.Properties = item.Properties;

        var updated = await _diagramService.UpdateDiagramAsync(diagram);
        return Ok(updated);
    }

    [HttpDelete("{userId}/items/{itemId}")]
    public async Task<ActionResult<Diagram>> DeleteItem(string userId, string itemId)
    {
        var diagram = await _diagramService.GetDiagramAsync(userId);
        
        if (diagram == null)
        {
            return NotFound("Diagram not found");
        }

        var item = diagram.Items.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            diagram.Items.Remove(item);
            var updated = await _diagramService.UpdateDiagramAsync(diagram);
            return Ok(updated);
        }

        return NotFound("Item not found in diagram");
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        await _diagramService.DeleteDiagramAsync(userId);
        return NoContent();
    }
}
