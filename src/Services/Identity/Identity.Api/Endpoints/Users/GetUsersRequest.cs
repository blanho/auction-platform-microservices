using BuildingBlocks.Application.Constants;

namespace Identity.Api.Endpoints.Users;

public sealed record GetUsersRequest(
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true,
    string? Search = null,
    string? Role = null,
    bool? IsActive = null,
    bool? IsSuspended = null);
