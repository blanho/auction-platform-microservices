namespace Identity.Application.Features.Auth.Queries.CheckUsernameAvailability;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record CheckUsernameAvailabilityQuery(string Username) : IQuery<bool>;

public class CheckUsernameAvailabilityQueryHandler(
    IUserService userService) : IQueryHandler<CheckUsernameAvailabilityQuery, bool>
{
    public async Task<Result<bool>> Handle(CheckUsernameAvailabilityQuery query, CancellationToken cancellationToken)
    {
        var user = await userService.FindByNameAsync(query.Username);
        return Result.Success(user == null);
    }
}
