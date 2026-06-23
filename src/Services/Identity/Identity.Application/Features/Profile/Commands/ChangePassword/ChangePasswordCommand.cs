namespace Identity.Application.Features.Profile.Commands.ChangePassword;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Profile;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record ChangePasswordCommand(string UserId, ChangePasswordRequest Request) : ICommand;

public class ChangePasswordCommandHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    IMediator mediator,
    BuildingBlocks.Application.Abstractions.Auditing.IAuditPublisher auditPublisher,
    ILogger<ChangePasswordCommandHandler> logger) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.WithIdentityErrors("Failed to change password", result.Errors));

        await mediator.Publish(new Identity.Domain.Events.PasswordChangedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        }, cancellationToken);

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new Identity.Application.DTOs.Audit.AuthAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = "PasswordChange",
                Success = true
            },
            BuildingBlocks.Application.Abstractions.Auditing.AuditAction.Updated,
            metadata: new Dictionary<string, object> { [BuildingBlocks.Application.Constants.AuditMetadataKeys.ActionLower] = Identity.Domain.Constants.IdentityDefaults.Audit.PasswordChange },
            cancellationToken: cancellationToken);

        logger.LogInformation("User {UserId} changed their password", command.UserId);

        return Result.Success();
    }
}
