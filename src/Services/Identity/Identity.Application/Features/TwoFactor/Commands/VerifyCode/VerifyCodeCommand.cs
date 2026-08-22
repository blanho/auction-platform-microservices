namespace Identity.Application.Features.TwoFactor.Commands.VerifyCode;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record VerifyCodeCommand(string Code, bool RememberDevice) : ICommand;

public class VerifyCodeCommandHandler(
    SignInManager<ApplicationUser> signInManager) : ICommandHandler<VerifyCodeCommand>
{
    public async Task<Result> Handle(VerifyCodeCommand command, CancellationToken cancellationToken)
    {
        var sanitizedCode = command.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            sanitizedCode,
            isPersistent: false,
            rememberClient: command.RememberDevice);

        if (result.Succeeded)
            return Result.Success();

        if (result.IsLockedOut)
            return Result.Failure(IdentityErrors.Auth.AccountLockedOut);

        return Result.Failure(IdentityErrors.TwoFactor.InvalidCode);
    }
}
