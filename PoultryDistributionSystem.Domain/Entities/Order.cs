using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Order entity representing customer orders
/// </summary>
public class Order : BaseEntity
{
    public Guid ShopId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime RequestedDeliveryDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedReason { get; set; }
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.None;

    // Navigation properties
    public virtual Shop Shop { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
