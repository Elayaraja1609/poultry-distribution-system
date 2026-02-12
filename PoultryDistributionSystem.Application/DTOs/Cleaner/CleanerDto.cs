namespace PoultryDistributionSystem.Application.DTOs.Cleaner;

/// <summary>
/// Cleaner DTO
/// </summary>
public class CleanerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
