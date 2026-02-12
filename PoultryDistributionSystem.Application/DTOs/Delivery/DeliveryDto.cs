using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Delivery;

/// <summary>
/// Delivery DTO
/// </summary>
public class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid DistributionId { get; set; }
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public int TotalQuantity { get; set; }
    public int VerifiedQuantity { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
