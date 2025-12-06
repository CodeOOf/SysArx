namespace SysArx.BuildingBlocks.Authentication.Settings;

/// <summary>
/// Authentication deployment mode
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// Local deployment with in-memory test users - for development and small teams
    /// </summary>
    Local,
    
    /// <summary>
    /// Enterprise deployment with direct LDAP authentication
    /// </summary>
    Ldap,
    
    /// <summary>
    /// Enterprise deployment with SSO provider (e.g., Keycloak) + LDAP federation
    /// </summary>
    LdapSSO
}

/// <summary>
/// Main authentication configuration
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Deployment mode: Local, Ldap, or LdapSSO
    /// </summary>
    public AuthenticationMode Mode { get; set; } = AuthenticationMode.Local;
    
    /// <summary>
    /// LDAP configuration (used in Ldap and LdapSSO modes)
    /// </summary>
    public LdapSettings Ldap { get; set; } = new();
    
    /// <summary>
    /// JWT configuration (used in Local and Ldap modes)
    /// </summary>
    public JwtSettings Jwt { get; set; } = new();
    
    /// <summary>
    /// SSO provider configuration (used in LdapSSO mode)
    /// </summary>
    public SsoSettings Sso { get; set; } = new();
}

public class LdapSettings
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 389;
    public bool UseSsl { get; set; } = false;
    public string BaseDn { get; set; } = "dc=sysarx,dc=local";
    public string BindDn { get; set; } = "cn=admin,dc=sysarx,dc=local";
    public string BindPassword { get; set; } = "admin";
    public string UserSearchBase { get; set; } = "ou=users,dc=sysarx,dc=local";
    public string UserSearchFilter { get; set; } = "(uid={0})";
    public int ConnectionTimeout { get; set; } = 5000;
    public int ReadTimeout { get; set; } = 10000;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = "YourSuperSecretKeyForDevelopmentThatIsAtLeast32Characters!";
    public string Issuer { get; set; } = "SysArx";
    public string Audience { get; set; } = "SysArxClients";
    public int ExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Generic SSO provider settings (Keycloak, Okta, Azure AD, etc.)
/// </summary>
public class SsoSettings
{
    /// <summary>
    /// SSO provider type (Keycloak, AzureAD, Okta, Custom)
    /// </summary>
    public string Provider { get; set; } = "Keycloak";
    
    /// <summary>
    /// OpenID Connect Authority URL
    /// </summary>
    public string Authority { get; set; } = "http://localhost:8080/realms/SysArxRealm";
    
    /// <summary>
    /// API audience identifier
    /// </summary>
    public string Audience { get; set; } = "sysarx-api";
    
    /// <summary>
    /// Client ID for API services
    /// </summary>
    public string ClientId { get; set; } = "sysarx-api";
    
    /// <summary>
    /// Client secret (if using confidential clients)
    /// </summary>
    public string ClientSecret { get; set; } = "";
    
    /// <summary>
    /// Require HTTPS metadata (set to true in production)
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = false;
    
    /// <summary>
    /// OpenID Configuration endpoint
    /// </summary>
    public string MetadataAddress { get; set; } = "http://localhost:8080/realms/SysArxRealm/.well-known/openid-configuration";
    
    /// <summary>
    /// Token validation clock skew in seconds
    /// </summary>
    public int ClockSkew { get; set; } = 30;
}
