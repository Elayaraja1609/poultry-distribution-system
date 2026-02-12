using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Notification;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Enums;
using PoultryDistributionSystem.Domain.Interfaces;
using OrderStatus = PoultryDistributionSystem.Domain.Enums.OrderStatus;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Notification service implementation
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService? _emailService;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService? emailService = null)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            RelatedEntityType = dto.RelatedEntityType,
            RelatedEntityId = dto.RelatedEntityId,
            IsRead = false
        };

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email notification
        try
        {
            if (_emailService != null)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId, cancellationToken);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        dto.Title,
                        dto.Message,
                        true,
                        cancellationToken);
                }
            }
        }
        catch
        {
            // Log error but don't fail the operation
        }

        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(
        Guid userId,
        bool? isRead,
        NotificationType? type,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var allNotifications = await _unitOfWork.Notifications.FindAsync(
            n => n.UserId == userId && !n.IsDeleted,
            cancellationToken);

        if (isRead.HasValue)
        {
            allNotifications = allNotifications.Where(n => n.IsRead == isRead.Value);
        }

        if (type.HasValue)
        {
            allNotifications = allNotifications.Where(n => n.Type == type.Value);
        }

        var notificationsList = allNotifications.OrderByDescending(n => n.CreatedAt).ToList();
        var totalCount = notificationsList.Count;
        var pagedNotifications = notificationsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<NotificationDto>
        {
            Items = _mapper.Map<List<NotificationDto>>(pagedNotifications),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.Notifications.FindAsync(
            n => n.UserId == userId && !n.IsRead && !n.IsDeleted,
            cancellationToken);
        return notifications.Count();
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId, cancellationToken);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Notifications.UpdateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.Notifications.FindAsync(
            n => n.UserId == userId && !n.IsRead && !n.IsDeleted,
            cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Notifications.UpdateAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendDeliveryScheduledNotificationAsync(Guid distributionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var distribution = await _unitOfWork.Distributions.GetByIdAsync(distributionId, cancellationToken);
        if (distribution == null) return;

        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Type = NotificationType.DeliveryScheduled,
            Title = "Delivery Scheduled",
            Message = $"A delivery has been scheduled for {distribution.ScheduledDate:MMM dd, yyyy}",
            RelatedEntityType = "Distribution",
            RelatedEntityId = distributionId
        };

        await CreateNotificationAsync(dto, cancellationToken);
    }

    public async Task SendPaymentReminderNotificationAsync(Guid saleId, Guid userId, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);
        if (sale == null) return;

        var payments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == saleId, cancellationToken);
        var paidAmount = payments.Sum(p => p.Amount);
        var remaining = sale.TotalAmount - paidAmount;

        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Type = NotificationType.PaymentReminder,
            Title = "Payment Reminder",
            Message = $"You have a pending payment of ${remaining:F2} for Sale #{sale.Id.ToString().Substring(0, 8)}",
            RelatedEntityType = "Sale",
            RelatedEntityId = saleId
        };

        await CreateNotificationAsync(dto, cancellationToken);
    }

    public async Task SendOrderStatusNotificationAsync(Guid orderId, Guid userId, OrderStatus status, string? reason, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null) return;

        var notificationType = status switch
        {
            OrderStatus.Approved => NotificationType.OrderApproved,
            OrderStatus.Rejected => NotificationType.OrderRejected,
            OrderStatus.Fulfilled => NotificationType.OrderFulfilled,
            _ => NotificationType.SystemAlert
        };

        var title = status switch
        {
            OrderStatus.Approved => "Order Approved",
            OrderStatus.Rejected => "Order Rejected",
            OrderStatus.Fulfilled => "Order Fulfilled",
            _ => "Order Status Update"
        };

        var message = status switch
        {
            OrderStatus.Approved => $"Your order #{order.Id.ToString().Substring(0, 8)} has been approved.",
            OrderStatus.Rejected => $"Your order #{order.Id.ToString().Substring(0, 8)} has been rejected. Reason: {reason ?? "Not specified"}",
            OrderStatus.Fulfilled => $"Your order #{order.Id.ToString().Substring(0, 8)} has been fulfilled.",
            _ => $"Your order #{order.Id.ToString().Substring(0, 8)} status has been updated to {status}."
        };

        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Type = notificationType,
            Title = title,
            Message = message,
            RelatedEntityType = "Order",
            RelatedEntityId = orderId
        };

        await CreateNotificationAsync(dto, cancellationToken);
    }
}
