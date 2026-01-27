using Notification.Application.DTOs;

namespace Notification.Application.Features.Notifications.CreateNotification;

public record CreateNotificationCommand(
    string UserId,
    string Type,
    string Title,
    string Message,
    string? Data = null,
    Guid? AuctionId = null,
    Guid? BidId = null
) : ICommand<NotificationDto>;
