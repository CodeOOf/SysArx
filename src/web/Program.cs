using SysArx.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Check authentication mode
var authMode = builder.Configuration.GetValue<string>("Authentication:Mode") ?? "Local";
var useSso = authMode.Equals("LdapSSO", StringComparison.OrdinalIgnoreCase);

if (useSso)
{
    // LdapSSO mode: Use SSO provider (Keycloak/Okta/Azure AD) for authentication
    var ssoProvider = builder.Configuration["Authentication:Sso:Provider"] ?? "SSO";
    Console.WriteLine($"Authentication Mode: LdapSSO ({ssoProvider})");
    
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Authentication:Sso:Authority"];
        options.ClientId = builder.Configuration["Authentication:Sso:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Sso:ClientSecret"];
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Authentication:Sso:RequireHttpsMetadata");
        options.GetClaimsFromUserInfoEndpoint = true;
        
        // Add scopes
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        
        // Map SSO roles
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "realm_access.roles"
        };
    });

    builder.Services.AddAuthorization();
}
else
{
    // Local or Ldap mode: No SSO authentication in Blazor (handled by Auth.API)
    Console.WriteLine($"Authentication Mode: {authMode} (using Auth.API)");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient for API calls
builder.Services.AddHttpClient("AuthAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuthApi"] ?? "http://localhost:5003");
});

builder.Services.AddHttpClient("SysMLStoreAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SysMLStoreApi"] ?? "http://localhost:5001");
});

builder.Services.AddHttpClient("SysMLDiagramAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SysMLDiagramApi"] ?? "http://localhost:5002");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (useSso)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
