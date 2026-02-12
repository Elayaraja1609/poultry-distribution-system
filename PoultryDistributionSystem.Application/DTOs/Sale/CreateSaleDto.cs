using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Sale;

/// <summary>
/// Create sale request DTO
/// </summary>
public class CreateSaleDto
{
    public Guid DeliveryId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}
