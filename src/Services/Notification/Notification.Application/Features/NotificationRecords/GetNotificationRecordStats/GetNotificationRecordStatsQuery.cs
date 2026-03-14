using Notification.Application.DTOs;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordStats;

public record GetNotificationRecordStatsQuery() : IQuery<NotificationRecordStatsDto>;
