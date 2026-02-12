namespace PoultryDistributionSystem.Application.DTOs.Driver;

/// <summary>
/// Create driver request DTO
/// </summary>
public class CreateDriverDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
}
