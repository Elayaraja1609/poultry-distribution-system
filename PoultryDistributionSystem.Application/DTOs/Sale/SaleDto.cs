using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Sale;

/// <summary>
/// Sale DTO
/// </summary>
public class SaleDto
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
