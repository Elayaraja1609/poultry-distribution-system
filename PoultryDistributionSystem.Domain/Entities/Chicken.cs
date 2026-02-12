using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Chicken entity representing batches of chickens
/// </summary>
public class Chicken : BaseEntity
{
    public string BatchNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid? FarmId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int Quantity { get; set; }
    public int AgeDays { get; set; } = 0;
    public decimal WeightKg { get; set; }
    public ChickenStatus Status { get; set; } = ChickenStatus.Purchased;
    public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;

    // Navigation properties
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual Farm? Farm { get; set; }
    public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public virtual ICollection<DistributionItem> DistributionItems { get; set; } = new List<DistributionItem>();
    public virtual ICollection<FarmOperation> FarmOperations { get; set; } = new List<FarmOperation>();
}
