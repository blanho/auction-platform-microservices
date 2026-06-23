namespace Identity.Application.Features.TwoFactor.Commands.UseRecoveryCode;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record UseRecoveryCodeCommand(string RecoveryCode) : ICommand;

public class UseRecoveryCodeCommandHandler(
    Microsoft.AspNetCore.Identity.SignInManager<Identity.Domain.Entities.ApplicationUser> signInManager,
    ILogger<UseRecoveryCodeCommandHandler> logger) : ICommandHandler<UseRecoveryCodeCommand>
{
    public async Task<Result> Handle(UseRecoveryCodeCommand command, CancellationToken cancellationToken)
    {
        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(command.RecoveryCode);

        if (result.Succeeded)
            return Result.Success();

        if (result.IsLockedOut)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.Auth.AccountLockedOut);

        return Result.Failure(Identity.Application.Errors.IdentityErrors.TwoFactor.InvalidRecoveryCode);
    }
}
