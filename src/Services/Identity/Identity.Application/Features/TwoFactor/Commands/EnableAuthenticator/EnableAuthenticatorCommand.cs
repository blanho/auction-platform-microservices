namespace Identity.Application.Features.TwoFactor.Commands.EnableAuthenticator;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record EnableAuthenticatorCommand(string UserId, string Code) : ICommand<RecoveryCodesResponse>;

public class EnableAuthenticatorCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMediator mediator,
    IAuditPublisher auditPublisher) : ICommandHandler<EnableAuthenticatorCommand, RecoveryCodesResponse>
{
    public async Task<Result<RecoveryCodesResponse>> Handle(EnableAuthenticatorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure<RecoveryCodesResponse>(IdentityErrors.User.NotFound);

        var verificationCode = command.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            verificationCode);

        if (!isValid)
            return Result.Failure<RecoveryCodesResponse>(IdentityErrors.TwoFactor.InvalidCode);

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await mediator.Publish(new TwoFactorEnabledDomainEvent
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
                Action = IdentityDefaults.AuditData.EnableTwoFactor,
                IsEnabled = true
            },
            AuditAction.Updated,
            metadata: new Dictionary<string, object> { [AuditMetadataKeys.ActionLower] = IdentityDefaults.Audit.TwoFactorEnabled });

        return Result.Success(new RecoveryCodesResponse
        {
            RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>()
        });
    }
}
