using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SysArx.Services;

public class AuthenticationStateService : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public AuthenticationStateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(_currentUser);
            }

            var username = await GetUsernameAsync();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username ?? "Unknown"),
                new Claim("token", token)
            }, "jwt");

            _currentUser = new ClaimsPrincipal(identity);
            return new AuthenticationState(_currentUser);
        }
        catch
        {
            return new AuthenticationState(_currentUser);
        }
    }

    public async Task SetAuthenticationAsync(string token, string username, bool rememberMe)
    {
        var storage = rememberMe ? "localStorage" : "sessionStorage";
        await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", "authToken", token);
        await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", "username", username);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("token", token)
        }, "jwt");

        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task ClearAuthenticationAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "username");
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "username");

        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token))
            {
                token = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "authToken");
            }
            return token;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> GetUsernameAsync()
    {
        try
        {
            var username = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "username");
            if (string.IsNullOrEmpty(username))
            {
                username = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "username");
            }
            return username;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
