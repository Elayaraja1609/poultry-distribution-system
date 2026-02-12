using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Delivery entity representing completed deliveries to shops
/// </summary>
public class Delivery : BaseEntity
{
    public Guid DistributionId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public int TotalQuantity { get; set; }
    public int VerifiedQuantity { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Distribution Distribution { get; set; } = null!;
    public virtual Shop Shop { get; set; } = null!;
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
