using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SysArx.BuildingBlocks.Healthchecks.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        return services;
    }

    public static void MapCustomHealthChecks(
        this WebApplication app,
        string healthPath = "/hc",
        string livenessPath = "/liveness",
        Func<HttpContext, HealthReport, Task>? responseWriter = null)
    {
        app.MapHealthChecks(healthPath, new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = responseWriter
        });

        app.MapHealthChecks(livenessPath, new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self")
        });
    }
}
