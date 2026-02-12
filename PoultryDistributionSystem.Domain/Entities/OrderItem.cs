using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Order item entity representing items in an order
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ChickenId { get; set; }
    public int RequestedQuantity { get; set; }
    public int FulfilledQuantity { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Chicken Chicken { get; set; } = null!;
}
