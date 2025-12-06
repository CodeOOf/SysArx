using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SysArx.BuildingBlocks.Authentication.Services;
using SysArx.BuildingBlocks.Authentication.Settings;

namespace SysArx.BuildingBlocks.Authentication.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds SysArx authentication services based on deployment mode
    /// Supports: Local (test users), Ldap (enterprise direct), LdapSSO (SSO + LDAP federation)
    /// </summary>
    public static IServiceCollection AddSysArxAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        var authSettings = configuration.GetSection("Authentication").Get<AuthenticationSettings>() 
            ?? new AuthenticationSettings();
        
        services.Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));
        
        // Register core services (used in all modes)
        services.AddSingleton<ILdapService, LdapService>();
        services.AddSingleton<ITokenService, TokenService>();

        // Log deployment mode
        var logger = services.BuildServiceProvider().GetService<ILogger<AuthenticationSettings>>();
        logger?.LogInformation("Authentication Mode: {Mode}", authSettings.Mode);

        // Configure authentication based on deployment mode
        switch (authSettings.Mode)
        {
            case AuthenticationMode.Local:
                ConfigureLocalAuthentication(services, authSettings, logger);
                break;
                
            case AuthenticationMode.Ldap:
                ConfigureLdapAuthentication(services, authSettings, logger);
                break;
                
            case AuthenticationMode.LdapSSO:
                ConfigureSsoAuthentication(services, authSettings, logger);
                break;
                
            default:
                throw new InvalidOperationException($"Unknown authentication mode: {authSettings.Mode}");
        }

        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// Local deployment mode: In-memory test users with custom JWT
    /// Use for: Development, demos, small teams
    /// </summary>
    private static void ConfigureLocalAuthentication(
        IServiceCollection services,
        AuthenticationSettings settings,
        ILogger? logger)
    {
        logger?.LogInformation("Configuring LOCAL authentication (test users + custom JWT)");
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(settings.Jwt.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = settings.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    /// <summary>
    /// LDAP deployment mode: Direct LDAP authentication with custom JWT
    /// Use for: Enterprise without SSO, simpler deployments
    /// </summary>
    private static void ConfigureLdapAuthentication(
        IServiceCollection services,
        AuthenticationSettings settings,
        ILogger? logger)
    {
        logger?.LogInformation("Configuring LDAP authentication (enterprise direct + custom JWT)");
        logger?.LogInformation("LDAP Server: {Server}:{Port}", settings.Ldap.Server, settings.Ldap.Port);
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(settings.Jwt.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = settings.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    /// <summary>
    /// LDAP+SSO deployment mode: SSO provider (Keycloak/Okta/Azure AD) with LDAP federation
    /// Use for: Enterprise with SSO, advanced security requirements
    /// </summary>
    private static void ConfigureSsoAuthentication(
        IServiceCollection services,
        AuthenticationSettings settings,
        ILogger? logger)
    {
        logger?.LogInformation("Configuring LDAP+SSO authentication ({Provider})", settings.Sso.Provider);
        logger?.LogInformation("SSO Authority: {Authority}", settings.Sso.Authority);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = settings.Sso.Authority;
                options.Audience = settings.Sso.Audience;
                options.RequireHttpsMetadata = settings.Sso.RequireHttpsMetadata;
                options.MetadataAddress = settings.Sso.MetadataAddress;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(settings.Sso.ClockSkew)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        logger?.LogError("SSO Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        logger?.LogDebug("SSO Token validated for user: {User}", 
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    }
                };
            });
    }

    /// <summary>
    /// Legacy compatibility method - maps to Local mode
    /// </summary>
    [Obsolete("Use AddSysArxAuthentication with Authentication.Mode configuration instead")]
    public static IServiceCollection AddLdapAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddSysArxAuthentication(configuration);
    }
}
