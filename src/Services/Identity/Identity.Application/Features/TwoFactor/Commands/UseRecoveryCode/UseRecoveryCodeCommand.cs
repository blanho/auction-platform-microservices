namespace Identity.Application.Features.TwoFactor.Commands.UseRecoveryCode;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record UseRecoveryCodeCommand(string RecoveryCode) : ICommand;

public class UseRecoveryCodeCommandHandler(
    SignInManager<ApplicationUser> signInManager) : ICommandHandler<UseRecoveryCodeCommand>
{
    public async Task<Result> Handle(UseRecoveryCodeCommand command, CancellationToken cancellationToken)
    {
        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(command.RecoveryCode);

        if (result.Succeeded)
            return Result.Success();

        if (result.IsLockedOut)
            return Result.Failure(IdentityErrors.Auth.AccountLockedOut);

        return Result.Failure(IdentityErrors.TwoFactor.InvalidRecoveryCode);
    }
}
