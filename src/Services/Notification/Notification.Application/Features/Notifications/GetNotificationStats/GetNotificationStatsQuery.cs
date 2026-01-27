using Notification.Application.DTOs;

namespace Notification.Application.Features.Notifications.GetNotificationStats;

public record GetNotificationStatsQuery() : IQuery<NotificationStatsDto>;
