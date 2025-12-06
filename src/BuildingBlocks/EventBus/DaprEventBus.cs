using Dapr.Client;
using SysArx.BuildingBlocks.EventBus.Abstractions;
using SysArx.BuildingBlocks.EventBus.Events;

namespace SysArx.BuildingBlocks.EventBus;

public class DaprEventBus : IEventBus
{
    private const string PubSubName = "pubsub";
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprEventBus> _logger;

    public DaprEventBus(DaprClient daprClient, ILogger<DaprEventBus> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task PublishAsync(IntegrationEvent @event)
    {
        var topicName = @event.GetType().Name;

        _logger.LogInformation(
            "Publishing event {@Event} to {PubSubName}.{TopicName}",
            @event,
            PubSubName,
            topicName);

        await _daprClient.PublishEventAsync(PubSubName, topicName, @event);
    }
}
