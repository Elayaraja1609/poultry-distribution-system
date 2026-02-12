using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Sale entity representing sales to shops
/// </summary>
public class Sale : BaseEntity
{
    public Guid DeliveryId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentGatewayTransactionId { get; set; }

    // Navigation properties
    public virtual Delivery Delivery { get; set; } = null!;
    public virtual Shop Shop { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
