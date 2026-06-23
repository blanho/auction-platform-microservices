namespace Identity.Application.Features.TwoFactor.Commands.DisableAuthenticator;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record DisableAuthenticatorCommand(string UserId, string Password) : ICommand;

public class DisableAuthenticatorCommandHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    IMediator mediator,
    BuildingBlocks.Application.Abstractions.Auditing.IAuditPublisher auditPublisher,
    ILogger<DisableAuthenticatorCommandHandler> logger) : ICommandHandler<DisableAuthenticatorCommand>
{
    public async Task<Result> Handle(DisableAuthenticatorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.Auth.InvalidPassword);

        var result = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.TwoFactor.DisableFailed);

        await userManager.ResetAuthenticatorKeyAsync(user);

        await mediator.Publish(new Identity.Domain.Events.TwoFactorDisabledDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        });

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new Identity.Application.DTOs.Audit.TwoFactorAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = "Disable2FA",
                IsEnabled = false
            },
            BuildingBlocks.Application.Abstractions.Auditing.AuditAction.Updated,
            metadata: new Dictionary<string, object> { [BuildingBlocks.Application.Constants.AuditMetadataKeys.ActionLower] = Identity.Domain.Constants.IdentityDefaults.Audit.TwoFactorDisabled });

        return Result.Success();
    }
}
