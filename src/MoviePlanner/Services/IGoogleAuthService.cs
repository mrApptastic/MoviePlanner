using MoviePlanner.Models;

namespace MoviePlanner.Services;

public interface IGoogleAuthService
{
    Task<bool> IsAuthenticatedAsync();
    Task<GoogleTokenInfo?> GetTokenInfoAsync();
    Task LoginAsync();
    Task LogoutAsync();
    Task<string> GetAccessTokenAsync();
}
