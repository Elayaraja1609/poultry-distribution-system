namespace PoultryDistributionSystem.Domain.Enums;

/// <summary>
/// Type of stock movement
/// </summary>
public enum StockMovementType
{
    In = 1,           // Stock coming in (purchase, transfer in)
    Out = 2,          // Stock going out (distribution, sale)
    Loss = 3,         // Stock loss (mortality, damage)
    Adjustment = 4    // Manual adjustment/correction
}
