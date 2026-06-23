namespace Identity.Application.Features.Profile.Commands.UpdateProfile;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Profile;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record UpdateProfileCommand(string UserId, UpdateProfileRequest Request) : ICommand<UserProfileDto>;

public class UpdateProfileCommandHandler(
    IUserService userService,
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    IMediator mediator,
    BuildingBlocks.Application.Abstractions.Auditing.IAuditPublisher auditPublisher,
    AutoMapper.IMapper mapper,
    ILogger<UpdateProfileCommandHandler> logger) : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var (user, roles) = await userService.GetByIdWithRolesAsync(command.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UserProfileDto>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var oldUserData = Identity.Application.DTOs.Audit.UserAuditData.FromUser(user, roles);

        user.FullName = request.FullName ?? user.FullName;
        user.Bio = request.Bio ?? user.Bio;
        user.Location = request.Location ?? user.Location;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result.Failure<UserProfileDto>(Identity.Application.Errors.IdentityErrors.WithIdentityErrors("Failed to update profile", result.Errors));

        logger.LogInformation("User {UserId} updated their profile", command.UserId);

        var profile = mapper.Map<UserProfileDto>(user);
        profile.Roles = roles.ToList();

        await mediator.Publish(new Identity.Domain.Events.UserUpdatedDomainEvent
        {
            UserId = profile.Id,
            Username = profile.Username,
            Email = profile.Email,
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber
        });

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            Identity.Application.DTOs.Audit.UserAuditData.FromUser(user, roles),
            BuildingBlocks.Application.Abstractions.Auditing.AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object> { [BuildingBlocks.Application.Constants.AuditMetadataKeys.ActionLower] = Identity.Domain.Constants.IdentityDefaults.Audit.ProfileUpdate },
            cancellationToken);

        return Result.Success(profile);
    }
}
