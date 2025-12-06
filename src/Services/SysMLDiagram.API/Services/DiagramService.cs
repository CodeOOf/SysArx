using Dapr.Client;
using SysArx.Services.SysMLDiagram.API.Models;

namespace SysArx.Services.SysMLDiagram.API.Services;

public interface IDiagramService
{
    Task<Diagram?> GetDiagramAsync(string userId);
    Task<Diagram> UpdateDiagramAsync(Diagram diagram);
    Task<bool> DeleteDiagramAsync(string userId);
}

public class DiagramService : IDiagramService
{
    private const string StateStoreName = "statestore";
    private readonly DaprClient _daprClient;
    private readonly ILogger<DiagramService> _logger;

    public DiagramService(DaprClient daprClient, ILogger<DiagramService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<Diagram?> GetDiagramAsync(string userId)
    {
        _logger.LogInformation("Getting diagram for user: {UserId}", userId);
        
        var diagram = await _daprClient.GetStateAsync<Diagram>(StateStoreName, userId);
        
        if (diagram == null)
        {
            _logger.LogInformation("No diagram found for user: {UserId}, creating new one", userId);
            diagram = new Diagram(userId);
        }

        return diagram;
    }

    public async Task<Diagram> UpdateDiagramAsync(Diagram diagram)
    {
        _logger.LogInformation("Updating diagram for user: {UserId}", diagram.UserId);
        
        diagram.ModifiedDate = DateTime.UtcNow;
        await _daprClient.SaveStateAsync(StateStoreName, diagram.UserId, diagram);
        
        _logger.LogInformation("Successfully updated diagram for user: {UserId}", diagram.UserId);
        
        return diagram;
    }

    public async Task<bool> DeleteDiagramAsync(string userId)
    {
        _logger.LogInformation("Deleting diagram for user: {UserId}", userId);
        
        await _daprClient.DeleteStateAsync(StateStoreName, userId);
        
        _logger.LogInformation("Successfully deleted diagram for user: {UserId}", userId);
        
        return true;
    }
}
