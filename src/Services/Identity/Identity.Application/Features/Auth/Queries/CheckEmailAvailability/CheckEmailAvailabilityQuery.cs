namespace Identity.Application.Features.Auth.Queries.CheckEmailAvailability;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record CheckEmailAvailabilityQuery(string Email) : IQuery<bool>;

public class CheckEmailAvailabilityQueryHandler(
    IUserService userService) : IQueryHandler<CheckEmailAvailabilityQuery, bool>
{
    public async Task<Result<bool>> Handle(CheckEmailAvailabilityQuery query, CancellationToken cancellationToken)
    {
        var user = await userService.FindByEmailAsync(query.Email);
        return Result.Success(user == null);
    }
}
