namespace Identity.Application.Features.Auth.Queries.GetCurrentUser;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record GetCurrentUserQuery(string UserId) : IQuery<UserDto>;

public class GetCurrentUserQueryHandler(
    IUserService userService,
    IMapper mapper) : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var (user, roles) = await userService.GetByIdWithRolesAsync(query.UserId);
        if (user == null)
            return Result.Failure<UserDto>(IdentityErrors.User.NotFound);

        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles = roles.ToList();
        return Result.Success(userDto);
    }
}
