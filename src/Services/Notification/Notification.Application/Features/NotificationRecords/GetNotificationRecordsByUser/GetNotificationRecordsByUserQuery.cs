using Notification.Application.DTOs;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordsByUser;

public record GetNotificationRecordsByUserQuery(Guid UserId, int Limit = 50) : IQuery<List<NotificationRecordDto>>;
