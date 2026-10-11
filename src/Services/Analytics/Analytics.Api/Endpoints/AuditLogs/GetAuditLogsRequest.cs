using Analytics.Domain.Enums;
using BuildingBlocks.Application.Constants;

namespace Analytics.Api.Endpoints.AuditLogs;

public sealed record GetAuditLogsRequest(
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true,
    Guid? EntityId = null,
    string? EntityType = null,
    Guid? UserId = null,
    string? ServiceName = null,
    AuditAction? Action = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null);
