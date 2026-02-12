using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Farm operation entity representing various farm activities
/// </summary>
public class FarmOperation : BaseEntity
{
    public Guid FarmId { get; set; }
    public Guid? ChickenId { get; set; }
    public OperationType OperationType { get; set; }
    public DateTime OperationDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Farm Farm { get; set; } = null!;
    public virtual Chicken? Chicken { get; set; }
}
