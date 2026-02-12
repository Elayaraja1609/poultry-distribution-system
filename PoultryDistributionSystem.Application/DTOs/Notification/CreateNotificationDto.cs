using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Notification;

/// <summary>
/// Create notification DTO
/// </summary>
public class CreateNotificationDto
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
}
