using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Distribution;

/// <summary>
/// Distribution DTO
/// </summary>
public class DistributionDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string CleanerName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DistributionStatus Status { get; set; }
    public int TotalItems { get; set; }
    public DateTime CreatedAt { get; set; }
}
