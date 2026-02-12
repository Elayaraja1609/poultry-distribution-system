namespace PoultryDistributionSystem.Application.DTOs.Auth;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
