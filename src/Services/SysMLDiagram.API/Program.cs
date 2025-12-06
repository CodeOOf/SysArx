using Serilog;
using SysArx.BuildingBlocks.EventBus.Extensions;
using SysArx.BuildingBlocks.Healthchecks.Extensions;
using SysArx.BuildingBlocks.Authentication.Extensions;
using SysArx.Services.SysMLDiagram.API.Services;
using AspNetCore.HealthChecks.UI.Client;

var appName = "SysMLDiagram API";
var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration.GetValue<string>("Seq:ServerUrl") ?? "http://localhost:5341"));

// Add authentication
builder.Services.AddSysArxAuthentication(builder.Configuration);

// Add services
builder.Services.AddDaprClient();
builder.Services.AddSingleton<IDiagramService, DiagramService>();
builder.Services.AddEventBus();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = appName, Version = "v1" });
});

// Add health checks
builder.Services.AddCustomHealthChecks();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} v1"));
}

var pathBase = builder.Configuration["PATH_BASE"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseCloudEvents();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.LocalRedirect("~/swagger"));
app.MapControllers();
app.MapSubscribeHandler();
app.MapCustomHealthChecks("/hc", "/liveness", UIResponseWriter.WriteHealthCheckUIResponse);

try
{
    app.Logger.LogInformation("Starting web host ({ApplicationName})...", appName);
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Host terminated unexpectedly ({ApplicationName})...", appName);
}
finally
{
    Log.CloseAndFlush();
}
