using Microsoft.AspNetCore.Mvc;
using SysArx.Services.SysMLStore.API.Models;
using SysArx.Services.SysMLStore.API.Services;

namespace SysArx.Services.SysMLStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SysMLItemsController : ControllerBase
{
    private readonly ISysMLStoreService _storeService;
    private readonly ILogger<SysMLItemsController> _logger;

    public SysMLItemsController(ISysMLStoreService storeService, ILogger<SysMLItemsController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<SysMLItem>>> GetAll()
    {
        var items = await _storeService.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SysMLItem>> GetById(string id)
    {
        var item = await _storeService.GetByIdAsync(id);
        
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<List<SysMLItem>>> GetByType(string type)
    {
        var items = await _storeService.GetByTypeAsync(type);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<SysMLItem>> Create(SysMLItem item)
    {
        // Get username from context (will be set by LDAP authentication)
        item.CreatedBy = User.Identity?.Name ?? "anonymous";
        
        var created = await _storeService.CreateAsync(item);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, SysMLItem item)
    {
        var existing = await _storeService.GetByIdAsync(id);
        
        if (existing == null)
        {
            return NotFound();
        }

        item.Id = id;
        item.CreatedDate = existing.CreatedDate;
        item.CreatedBy = existing.CreatedBy;
        
        await _storeService.UpdateAsync(id, item);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _storeService.GetByIdAsync(id);
        
        if (existing == null)
        {
            return NotFound();
        }

        await _storeService.DeleteAsync(id);
        return NoContent();
    }
}
