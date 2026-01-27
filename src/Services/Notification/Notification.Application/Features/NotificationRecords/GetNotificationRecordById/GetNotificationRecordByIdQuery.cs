using Notification.Application.DTOs;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordById;

public record GetNotificationRecordByIdQuery(Guid Id) : IQuery<NotificationRecordDto?>;
