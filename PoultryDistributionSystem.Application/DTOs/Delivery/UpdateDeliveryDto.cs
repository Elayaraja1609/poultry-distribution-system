using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Delivery;

/// <summary>
/// Update delivery request DTO
/// </summary>
public class UpdateDeliveryDto
{
    public int VerifiedQuantity { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string? Notes { get; set; }
}
