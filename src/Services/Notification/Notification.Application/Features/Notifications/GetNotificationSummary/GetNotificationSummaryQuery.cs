using Notification.Application.DTOs;

namespace Notification.Application.Features.Notifications.GetNotificationSummary;

public record GetNotificationSummaryQuery(string UserId) : IQuery<NotificationSummaryDto>;
