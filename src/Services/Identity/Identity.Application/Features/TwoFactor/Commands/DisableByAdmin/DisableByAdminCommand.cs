namespace Identity.Application.Features.TwoFactor.Commands.DisableByAdmin;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record DisableByAdminCommand(string UserId) : ICommand;

public class DisableByAdminCommandHandler(
    UserManager<ApplicationUser> userManager) : ICommandHandler<DisableByAdminCommand>
{
    public async Task<Result> Handle(DisableByAdminCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return Result.Failure(IdentityErrors.TwoFactor.NotEnabled);

        var result = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.TwoFactor.DisableFailed);

        await userManager.ResetAuthenticatorKeyAsync(user);
        return Result.Success();
    }
}
