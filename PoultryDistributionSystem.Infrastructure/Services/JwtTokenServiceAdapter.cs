using System.Security.Claims;
using PoultryDistributionSystem.Application.Interfaces;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// Adapter to bridge Infrastructure.IJwtTokenService to Application.IJwtTokenService
/// </summary>
public class JwtTokenServiceAdapter : Application.Interfaces.IJwtTokenService
{
    private readonly Services.Interfaces.IJwtTokenService _jwtTokenService;

    public JwtTokenServiceAdapter(Services.Interfaces.IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        return _jwtTokenService.GenerateAccessToken(claims);
    }

    public string GenerateRefreshToken()
    {
        return _jwtTokenService.GenerateRefreshToken();
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        return _jwtTokenService.GetPrincipalFromExpiredToken(token);
    }

    public DateTime GetRefreshTokenExpiry()
    {
        return _jwtTokenService.GetRefreshTokenExpiry();
    }
}
