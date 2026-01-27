using Notification.Application.DTOs;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecords;

public record GetNotificationRecordsQuery(
    Guid? UserId,
    string? Channel,
    string? Status,
    string? TemplateKey,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page = 1,
    int PageSize = 20
) : IQuery<PaginatedResult<NotificationRecordDto>>;
