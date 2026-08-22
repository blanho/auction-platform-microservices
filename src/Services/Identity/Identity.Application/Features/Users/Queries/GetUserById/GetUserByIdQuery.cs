namespace Identity.Application.Features.Users.Queries.GetUserById;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;

public record GetUserByIdQuery(string Id) : IQuery<AdminUserDto?>;

public class GetUserByIdQueryHandler(
    IUserService userService,
    IMapper mapper) : IQueryHandler<GetUserByIdQuery, AdminUserDto?>
{
    public async Task<Result<AdminUserDto?>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var (user, roles) = await userService.GetByIdWithRolesAsync(query.Id, cancellationToken);
        if (user == null)
            return Result.Success<AdminUserDto?>(null);

        var dto = mapper.Map<AdminUserDto>(user);
        dto.Roles = roles.ToList();
        return Result.Success<AdminUserDto?>(dto);
    }
}
