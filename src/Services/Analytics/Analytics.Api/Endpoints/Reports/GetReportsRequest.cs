using Analytics.Domain.Enums;
using BuildingBlocks.Application.Constants;

namespace Analytics.Api.Endpoints.Reports;

public sealed record GetReportsRequest(
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true,
    ReportStatus? Status = null,
    ReportType? Type = null,
    ReportPriority? Priority = null,
    string? ReportedUsername = null);
