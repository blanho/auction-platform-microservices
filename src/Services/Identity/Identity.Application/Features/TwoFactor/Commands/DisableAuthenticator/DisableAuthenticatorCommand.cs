namespace Identity.Application.Features.TwoFactor.Commands.DisableAuthenticator;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record DisableAuthenticatorCommand(string UserId, string Password) : ICommand;

public class DisableAuthenticatorCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMediator mediator,
    IAuditPublisher auditPublisher) : ICommandHandler<DisableAuthenticatorCommand>
{
    public async Task<Result> Handle(DisableAuthenticatorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return Result.Failure(IdentityErrors.Auth.InvalidPassword);

        var result = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.TwoFactor.DisableFailed);

        await userManager.ResetAuthenticatorKeyAsync(user);

        await mediator.Publish(new TwoFactorDisabledDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        });

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new TwoFactorAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = IdentityDefaults.AuditData.DisableTwoFactor,
                IsEnabled = false
            },
            AuditAction.Updated,
            metadata: new Dictionary<string, object> { [AuditMetadataKeys.ActionLower] = IdentityDefaults.Audit.TwoFactorDisabled });

        return Result.Success();
    }
}
