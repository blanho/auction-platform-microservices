namespace Identity.Application.Features.TwoFactor.Commands.EnableAuthenticator;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record EnableAuthenticatorCommand(string UserId, string Code) : ICommand<RecoveryCodesResponse>;

public class EnableAuthenticatorCommandHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    IMediator mediator,
    BuildingBlocks.Application.Abstractions.Auditing.IAuditPublisher auditPublisher,
    ILogger<EnableAuthenticatorCommandHandler> logger) : ICommandHandler<EnableAuthenticatorCommand, RecoveryCodesResponse>
{
    public async Task<Result<RecoveryCodesResponse>> Handle(EnableAuthenticatorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure<RecoveryCodesResponse>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var verificationCode = command.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            verificationCode);

        if (!isValid)
            return Result.Failure<RecoveryCodesResponse>(Identity.Application.Errors.IdentityErrors.TwoFactor.InvalidCode);

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await mediator.Publish(new Identity.Domain.Events.TwoFactorEnabledDomainEvent
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
                Action = "Enable2FA",
                IsEnabled = true
            },
            BuildingBlocks.Application.Abstractions.Auditing.AuditAction.Updated,
            metadata: new Dictionary<string, object> { [BuildingBlocks.Application.Constants.AuditMetadataKeys.ActionLower] = Identity.Domain.Constants.IdentityDefaults.Audit.TwoFactorEnabled });

        return Result.Success(new RecoveryCodesResponse
        {
            RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>()
        });
    }
}
