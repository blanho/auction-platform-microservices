namespace Identity.Application.Features.Profile.Commands.ChangePassword;

using Identity.Application.DTOs.Auth;
using MediatR;

public record ChangePasswordCommand(string UserId, ChangePasswordRequest Request) : ICommand;

public class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMediator mediator,
    IAuditPublisher auditPublisher,
    ILogger<ChangePasswordCommandHandler> logger) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.WithIdentityErrors("Failed to change password", result.Errors));

        await mediator.Publish(new PasswordChangedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        }, cancellationToken);

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new AuthAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = IdentityDefaults.AuditData.PasswordChange,
                Success = true
            },
            AuditAction.Updated,
            metadata: new Dictionary<string, object> { [AuditMetadataKeys.ActionLower] = IdentityDefaults.Audit.PasswordChange },
            cancellationToken: cancellationToken);

        logger.LogInformation("User {UserId} changed their password", command.UserId);

        return Result.Success();
    }
}
