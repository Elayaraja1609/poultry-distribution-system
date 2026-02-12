using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Distribution entity representing scheduled distributions
/// </summary>
public class Distribution : BaseEntity
{
    public Guid VehicleId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DistributionStatus Status { get; set; } = DistributionStatus.Scheduled;

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
    public virtual ICollection<DistributionItem> DistributionItems { get; set; } = new List<DistributionItem>();
    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
