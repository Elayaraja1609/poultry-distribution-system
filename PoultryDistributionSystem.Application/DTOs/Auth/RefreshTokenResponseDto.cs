namespace PoultryDistributionSystem.Application.DTOs.Auth;

/// <summary>
/// Refresh token response DTO
/// </summary>
public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
