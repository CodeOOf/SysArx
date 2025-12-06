using SysArx.BuildingBlocks.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace SysArx.BuildingBlocks.EventBus.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, DaprEventBus>();
        return services;
    }
}
