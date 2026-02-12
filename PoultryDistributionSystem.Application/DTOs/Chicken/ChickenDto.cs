using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Chicken;

/// <summary>
/// Chicken DTO
/// </summary>
public class ChickenDto
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid? FarmId { get; set; }
    public string? FarmName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int Quantity { get; set; }
    public int AgeDays { get; set; }
    public decimal WeightKg { get; set; }
    public ChickenStatus Status { get; set; }
    public HealthStatus HealthStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
