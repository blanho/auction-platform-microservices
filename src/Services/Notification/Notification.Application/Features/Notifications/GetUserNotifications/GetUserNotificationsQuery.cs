using Notification.Application.DTOs;

namespace Notification.Application.Features.Notifications.GetUserNotifications;

public record GetUserNotificationsQuery(
    string UserId,
    int Page = 1,
    int PageSize = 20
) : IQuery<PaginatedResult<NotificationDto>>;
