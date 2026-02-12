using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Stock movement entity for tracking inventory changes
/// </summary>
public class StockMovement : BaseEntity
{
    public Guid FarmId { get; set; }
    public Guid ChickenId { get; set; }
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Farm Farm { get; set; } = null!;
    public virtual Chicken Chicken { get; set; } = null!;
}
