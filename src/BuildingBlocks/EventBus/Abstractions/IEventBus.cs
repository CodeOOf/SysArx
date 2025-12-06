using SysArx.BuildingBlocks.EventBus.Events;

namespace SysArx.BuildingBlocks.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event);
}
