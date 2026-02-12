using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Purchase item entity representing individual items in a purchase
/// </summary>
public class PurchaseItem : BaseEntity
{
    public Guid PurchaseId { get; set; }
    public Guid ChickenId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation properties
    public virtual Purchase Purchase { get; set; } = null!;
    public virtual Chicken Chicken { get; set; } = null!;
}
