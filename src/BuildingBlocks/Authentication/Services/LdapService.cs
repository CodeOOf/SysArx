using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysArx.BuildingBlocks.Authentication.Models;
using SysArx.BuildingBlocks.Authentication.Settings;

namespace SysArx.BuildingBlocks.Authentication.Services;

public interface ILdapService
{
    Task<UserInfo?> AuthenticateAsync(string username, string password);
    Task<UserInfo?> GetUserInfoAsync(string username);
    List<(string username, string password, string displayName, string email)> GetTestUsers();
}

public class LdapService : ILdapService
{
    private readonly AuthenticationSettings _authSettings;
    private readonly ILogger<LdapService> _logger;

    // Test users for Local deployment mode
    private readonly Dictionary<string, (string password, string displayName, string email)> _testUsers = new()
    {
        { "admin", ("admin123", "Administrator", "admin@sysarx.local") },
        { "user1", ("user123", "John Doe", "john.doe@sysarx.local") },
        { "user2", ("user123", "Jane Smith", "jane.smith@sysarx.local") },
        { "developer", ("dev123", "Dev User", "dev@sysarx.local") },
        { "architect", ("arch123", "System Architect", "architect@sysarx.local") }
    };

    public LdapService(IOptions<AuthenticationSettings> authSettings, ILogger<LdapService> logger)
    {
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    public async Task<UserInfo?> AuthenticateAsync(string username, string password)
    {
        // Local mode: Use test users
        if (_authSettings.Mode == AuthenticationMode.Local)
        {
            _logger.LogInformation("LOCAL mode: Authenticating user {Username} with test users", username);
            
            if (_testUsers.TryGetValue(username.ToLower(), out var testUser))
            {
                if (testUser.password == password)
                {
                    return new UserInfo
                    {
                        Username = username,
                        DisplayName = testUser.displayName,
                        Email = testUser.email,
                        Groups = new List<string> { "users", username == "admin" ? "admins" : "developers" }
                    };
                }
            }
            
            _logger.LogWarning("Authentication failed for user: {Username}", username);
            return null;
        }

        // LDAP mode: Direct LDAP authentication
        if (_authSettings.Mode == AuthenticationMode.Ldap)
        {
            _logger.LogInformation("LDAP mode: Authenticating user {Username} against LDAP server", username);
            return await AuthenticateWithLdapAsync(username, password);
        }

        // LdapSSO mode: Authentication handled by SSO provider, this is fallback
        if (_authSettings.Mode == AuthenticationMode.LdapSSO)
        {
            _logger.LogWarning("LdapSSO mode: Direct LDAP authentication called - user should authenticate via SSO provider");
            return await AuthenticateWithLdapAsync(username, password);
        }

        _logger.LogError("Unknown authentication mode: {Mode}", _authSettings.Mode);
        return null;
    }

    public List<(string username, string password, string displayName, string email)> GetTestUsers()
    {
        return _testUsers.Select(kv => (
            username: kv.Key,
            password: kv.Value.password,
            displayName: kv.Value.displayName,
            email: kv.Value.email
        )).ToList();
    }

    private async Task<UserInfo?> AuthenticateWithLdapAsync(string username, string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                var ldapSettings = _authSettings.Ldap;
                using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapSettings.Server, ldapSettings.Port));
                connection.SessionOptions.ProtocolVersion = 3;
                connection.AuthType = AuthType.Basic;
                connection.Timeout = TimeSpan.FromMilliseconds(ldapSettings.ConnectionTimeout);

                if (ldapSettings.UseSsl)
                {
                    connection.SessionOptions.SecureSocketLayer = true;
                }

                var userDn = string.Format(ldapSettings.UserSearchFilter, username);
                var fullUserDn = $"uid={username},{ldapSettings.UserSearchBase}";

                try
                {
                    connection.Bind(new NetworkCredential(fullUserDn, password));
                    _logger.LogInformation("LDAP authentication successful for user: {Username}", username);

                    // Get user info
                    var searchRequest = new SearchRequest(
                        ldapSettings.UserSearchBase,
                        string.Format(ldapSettings.UserSearchFilter, username),
                        SearchScope.Subtree,
                        new[] { "cn", "mail", "memberOf" }
                    );

                    var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                    
                    if (searchResponse.Entries.Count > 0)
                    {
                        var entry = searchResponse.Entries[0];
                        
                        return new UserInfo
                        {
                            Username = username,
                            DisplayName = entry.Attributes["cn"]?[0]?.ToString() ?? username,
                            Email = entry.Attributes["mail"]?[0]?.ToString() ?? $"{username}@sysarx.local",
                            Groups = entry.Attributes["memberOf"]?.GetValues(typeof(string))
                                .Cast<string>()
                                .ToList() ?? new List<string>()
                        };
                    }
                }
                catch (LdapException ex)
                {
                    _logger.LogWarning(ex, "LDAP authentication failed for user: {Username}", username);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during LDAP authentication for user: {Username}", username);
                return null;
            }

            return null;
        });
    }

    public async Task<UserInfo?> GetUserInfoAsync(string username)
    {
        // Local mode: Return test user info
        if (_authSettings.Mode == AuthenticationMode.Local)
        {
            if (_testUsers.TryGetValue(username.ToLower(), out var testUser))
            {
                return new UserInfo
                {
                    Username = username,
                    DisplayName = testUser.displayName,
                    Email = testUser.email,
                    Groups = new List<string> { "users" }
                };
            }
            return null;
        }

        // LDAP/LdapSSO mode: Query LDAP for user info
        _logger.LogInformation("Retrieving user info from LDAP for: {Username}", username);
        return await Task.FromResult<UserInfo?>(null);
    }
}
