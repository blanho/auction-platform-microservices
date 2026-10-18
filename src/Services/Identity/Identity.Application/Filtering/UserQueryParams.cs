using BuildingBlocks.Application.Paging;

namespace Identity.Application.Filtering;

public sealed class UserQueryParams : QueryParameters<UserFilter>
{
    public string? Role { get; init; }
}
