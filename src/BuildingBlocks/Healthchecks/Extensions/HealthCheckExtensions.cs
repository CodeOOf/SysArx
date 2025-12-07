using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SysArx.BuildingBlocks.Healthchecks.Extensions;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddCustomHealthChecks(this IServiceCollection services)
    {
        return services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());
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
