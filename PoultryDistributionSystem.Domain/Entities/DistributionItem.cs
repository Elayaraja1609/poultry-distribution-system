using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Distribution item entity representing individual items in a distribution
/// </summary>
public class DistributionItem : BaseEntity
{
    public Guid DistributionId { get; set; }
    public Guid ChickenId { get; set; }
    public Guid ShopId { get; set; }
    public int Quantity { get; set; }
    public DistributionItemStatus DeliveryStatus { get; set; } = DistributionItemStatus.Pending;
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Distribution Distribution { get; set; } = null!;
    public virtual Chicken Chicken { get; set; } = null!;
    public virtual Shop Shop { get; set; } = null!;
}
