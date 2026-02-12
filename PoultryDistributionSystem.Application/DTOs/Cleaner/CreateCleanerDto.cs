namespace PoultryDistributionSystem.Application.DTOs.Cleaner;

/// <summary>
/// Create cleaner request DTO
/// </summary>
public class CreateCleanerDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
