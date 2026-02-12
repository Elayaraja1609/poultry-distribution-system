using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Chicken;

/// <summary>
/// Update chicken request DTO
/// </summary>
public class UpdateChickenDto
{
    public Guid? FarmId { get; set; }
    public int AgeDays { get; set; }
    public decimal WeightKg { get; set; }
    public ChickenStatus Status { get; set; }
    public HealthStatus HealthStatus { get; set; }
}
