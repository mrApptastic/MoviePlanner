using System.Text.Json;
using Microsoft.JSInterop;
using MoviePlanner.Models;

namespace MoviePlanner.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IJSRuntime _jsRuntime;
    private GoogleTokenInfo? _tokenInfo;
    private const string TokenStorageKey = "google_token_info";

    public GoogleAuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenInfoAsync();
        return token != null && token.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<GoogleTokenInfo?> GetTokenInfoAsync()
    {
        if (_tokenInfo != null && _tokenInfo.ExpiresAt > DateTime.UtcNow)
            return _tokenInfo;

        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenStorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                _tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json);
                if (_tokenInfo != null && _tokenInfo.ExpiresAt > DateTime.UtcNow)
                    return _tokenInfo;
            }
        }
        catch { }

        return null;
    }

    public async Task LoginAsync()
    {
        await _jsRuntime.InvokeVoidAsync("googleAuth.signIn");
    }

    public async Task LogoutAsync()
    {
        _tokenInfo = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var token = await GetTokenInfoAsync();
        if (token == null)
            throw new InvalidOperationException("User is not authenticated.");
        return token.AccessToken;
    }

    [JSInvokable]
    public async Task OnGoogleSignIn(string tokenInfoJson)
    {
        _tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(tokenInfoJson);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, tokenInfoJson);
    }
}
