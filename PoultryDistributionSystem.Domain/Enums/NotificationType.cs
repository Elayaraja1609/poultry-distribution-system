namespace PoultryDistributionSystem.Domain.Enums;

/// <summary>
/// Notification type enum
/// </summary>
public enum NotificationType
{
    DeliveryScheduled = 1,
    PaymentReminder = 2,
    OrderApproved = 3,
    OrderRejected = 4,
    OrderFulfilled = 5,
    LowStock = 6,
    SystemAlert = 7
}
