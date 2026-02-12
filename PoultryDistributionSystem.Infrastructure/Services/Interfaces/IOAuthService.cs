namespace PoultryDistributionSystem.Infrastructure.Services.Interfaces;

/// <summary>
/// OAuth service interface for external authentication
/// </summary>
public interface IOAuthService
{
    Task<OAuthUserInfo?> GetUserInfoAsync(string provider, string accessToken, CancellationToken cancellationToken = default);
    string GetAuthorizationUrl(string provider, string redirectUri);
}

/// <summary>
/// OAuth user information
/// </summary>
public class OAuthUserInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Picture { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
}
