namespace PoultryDistributionSystem.Domain.Enums;

/// <summary>
/// Status of chickens in the system
/// </summary>
public enum ChickenStatus
{
    Purchased = 1,
    InFarm = 2,
    ReadyForDistribution = 3,
    InTransit = 4,
    Delivered = 5
}
