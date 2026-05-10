using Notification.Application.DTOs;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecords;

public record GetNotificationRecordsQuery(
    Guid? UserId,
    string? Channel,
    string? Status,
    string? TemplateKey,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page = NotificationDefaults.Pagination.DefaultPage,
    int PageSize = NotificationDefaults.Pagination.DefaultPageSize
) : IQuery<PaginatedResult<NotificationRecordDto>>;
