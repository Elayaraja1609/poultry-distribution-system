using System.Security.Claims;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// JWT token service interface (abstraction for Application layer)
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetRefreshTokenExpiry();
}
