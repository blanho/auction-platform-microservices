namespace Identity.Application.Features.Profile.Queries.GetProfile;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Profile;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetProfileQuery(string UserId) : IQuery<UserProfileDto>;

public class GetProfileQueryHandler(
    IUserService userService,
    AutoMapper.IMapper mapper,
    ILogger<GetProfileQueryHandler> logger) : IQueryHandler<GetProfileQuery, UserProfileDto>
{
    public async Task<Result<UserProfileDto>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
    {
        var (user, roles) = await userService.GetByIdWithRolesAsync(query.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UserProfileDto>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var profile = mapper.Map<UserProfileDto>(user);
        profile.Roles = roles.ToList();

        return Result.Success(profile);
    }
}
