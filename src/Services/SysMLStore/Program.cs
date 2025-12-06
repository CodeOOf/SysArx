using Serilog;
using SysArx.BuildingBlocks.EventBus.Extensions;
using SysArx.BuildingBlocks.Healthchecks.Extensions;
using SysArx.BuildingBlocks.Authentication.Extensions;
using SysArx.Services.SysMLStore.Services;
using SysArx.Services.SysMLStore.Settings;
using SysArx.Services.SysMLStore.Storage;
using AspNetCore.HealthChecks.UI.Client;

var appName = "SysMLStore API";
var builder = WebApplication.CreateBuilder(args);

// Add Dapr configuration
builder.Configuration.AddDaprSecretStore(
    "secretstore",
    new DaprClientBuilder().Build());

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration.GetValue<string>("Seq:ServerUrl") ?? "http://localhost:5341"));

// Add authentication
builder.Services.AddSysArxAuthentication(builder.Configuration);

// Add storage configuration
builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));

// Configure MongoDB settings if using NoSQL storage
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Register storage provider based on configuration
var storageProvider = builder.Configuration.GetValue<string>("Storage:Provider") ?? "NoSQL";
Console.WriteLine($"Storage Provider: {storageProvider}");

switch (storageProvider.ToLower())
{
    case "github":
        builder.Services.AddHttpClient("GitHub");
        builder.Services.AddSingleton<IStorageProvider, GitHubStorageProvider>();
        break;
    case "gitlab":
        builder.Services.AddHttpClient("GitLab");
        builder.Services.AddSingleton<IStorageProvider, GitLabStorageProvider>();
        break;
    case "nosql":
    default:
        builder.Services.AddSingleton<IStorageProvider, NoSqlStorageProvider>();
        break;
}

builder.Services.AddSingleton<ISysMLStoreService, SysMLStoreService>();
builder.Services.AddDaprClient();
builder.Services.AddEventBus();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = appName, Version = "v1" });
});

// Add health checks
builder.Services.AddCustomHealthChecks()
    .AddMongoDb(
        builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString") ?? "mongodb://localhost:27017",
        name: "mongodb",
        tags: new[] { "ready" });

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
