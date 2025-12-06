using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SysArx.BuildingBlocks.Authentication.Models;
using SysArx.BuildingBlocks.Authentication.Services;
using SysArx.BuildingBlocks.Authentication.Settings;

namespace SysArx.Services.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILdapService _ldapService;
    private readonly ITokenService _tokenService;
    private readonly AuthenticationSettings _authSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ILdapService ldapService,
        ITokenService tokenService,
        IOptions<AuthenticationSettings> authSettings,
        ILogger<AuthController> logger)
    {
        _ldapService = ldapService;
        _tokenService = tokenService;
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username} (Mode: {Mode})", 
            request.Username, _authSettings.Mode);

        var userInfo = await _ldapService.AuthenticateAsync(request.Username, request.Password);

        if (userInfo == null)
        {
            _logger.LogWarning("Login failed for user: {Username}", request.Username);
            return Ok(new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }

        var token = _tokenService.GenerateToken(userInfo);

        _logger.LogInformation("Login successful for user: {Username}", request.Username);

        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            Username = userInfo.Username,
            DisplayName = userInfo.DisplayName,
            Email = userInfo.Email,
            Message = "Login successful"
        });
    }

    [HttpGet("mode")]
    public ActionResult<object> GetAuthenticationMode()
    {
        return Ok(new
        {
            Mode = _authSettings.Mode.ToString(),
            Description = _authSettings.Mode switch
            {
                AuthenticationMode.Local => "Local deployment with test users for development",
                AuthenticationMode.Ldap => "Enterprise deployment with direct LDAP authentication",
                AuthenticationMode.LdapSSO => $"Enterprise deployment with SSO ({_authSettings.Sso.Provider}) + LDAP federation",
                _ => "Unknown"
            },
            SsoProvider = _authSettings.Mode == AuthenticationMode.LdapSSO ? _authSettings.Sso.Provider : null
        });
    }

    [HttpGet("test-users")]
    public ActionResult<object> GetTestUsers()
    {
        if (_authSettings.Mode != AuthenticationMode.Local)
        {
            return Ok(new
            {
                Message = $"Test users are only available in Local mode. Current mode: {_authSettings.Mode}",
                Users = Array.Empty<object>()
            });
        }

        var users = _ldapService.GetTestUsers();
        return Ok(new
        {
            Message = "Available test users for local development",
            Mode = "Local",
            Users = users.Select(u => new 
            { 
                u.username, 
                u.password, 
                u.displayName,
                u.email
            })
        });
    }
}
