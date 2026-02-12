using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Notification;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Notification service interface
/// </summary>
public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead, NotificationType? type, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SendDeliveryScheduledNotificationAsync(Guid distributionId, Guid userId, CancellationToken cancellationToken = default);
    Task SendPaymentReminderNotificationAsync(Guid saleId, Guid userId, CancellationToken cancellationToken = default);
    Task SendOrderStatusNotificationAsync(Guid orderId, Guid userId, OrderStatus status, string? reason, CancellationToken cancellationToken = default);
}
