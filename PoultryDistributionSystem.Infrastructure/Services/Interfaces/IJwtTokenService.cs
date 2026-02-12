using System.Security.Claims;

namespace PoultryDistributionSystem.Infrastructure.Services.Interfaces;

/// <summary>
/// JWT token service interface for token generation and validation
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetRefreshTokenExpiry();
}
