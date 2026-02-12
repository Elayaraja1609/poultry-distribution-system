namespace PoultryDistributionSystem.Domain.Enums;

/// <summary>
/// Status of distributions
/// </summary>
public enum DistributionStatus
{
    Scheduled = 1,
    InTransit = 2,
    Completed = 3,
    Cancelled = 4
}
