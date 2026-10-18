using Identity.Application.Filtering;

namespace Identity.Application.Features.Users.Queries.GetUsers;

public record GetUsersListQuery(UserQueryParams Query) : IQuery<PaginatedResult<AdminUserDto>>;

public class GetUsersListQueryHandler(IUserService userService)
    : IQueryHandler<GetUsersListQuery, PaginatedResult<AdminUserDto>>
{
    public async Task<Result<PaginatedResult<AdminUserDto>>> Handle(
        GetUsersListQuery request, CancellationToken cancellationToken) =>
        Result.Success(await userService.GetUsersPagedAsync(request.Query, cancellationToken));
}
