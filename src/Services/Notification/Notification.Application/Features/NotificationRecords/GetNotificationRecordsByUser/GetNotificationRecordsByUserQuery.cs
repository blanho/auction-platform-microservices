using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using Notification.Application.DTOs;
using Notification.Domain.Entities;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordsByUser;

public record GetNotificationRecordsByUserQuery(
    Guid UserId, 
    string? Channel = null,
    NotificationRecordStatus? Status = null,
    string? TemplateKey = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<NotificationRecordDto>>;
