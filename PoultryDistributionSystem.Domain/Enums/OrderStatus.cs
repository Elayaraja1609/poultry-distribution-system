namespace PoultryDistributionSystem.Domain.Enums;

/// <summary>
/// Order status enum
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Processing = 4,
    PartiallyFulfilled = 5,
    Fulfilled = 6,
    Cancelled = 7
}
