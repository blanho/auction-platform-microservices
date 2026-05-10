using Notification.Application.DTOs;

namespace Notification.Application.Features.Notifications.GetUserNotifications;

public record GetUserNotificationsQuery(
    string UserId,
    int Page = NotificationDefaults.Pagination.DefaultPage,
    int PageSize = NotificationDefaults.Pagination.DefaultPageSize
) : IQuery<PaginatedResult<NotificationDto>>;
