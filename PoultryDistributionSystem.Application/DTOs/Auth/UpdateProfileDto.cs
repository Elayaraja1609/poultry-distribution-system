namespace PoultryDistributionSystem.Application.DTOs.Auth;

/// <summary>
/// Update profile request DTO
/// </summary>
public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
