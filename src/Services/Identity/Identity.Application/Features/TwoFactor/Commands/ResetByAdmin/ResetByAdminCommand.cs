namespace Identity.Application.Features.TwoFactor.Commands.ResetByAdmin;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record ResetByAdminCommand(string UserId) : ICommand;

public class ResetByAdminCommandHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    ILogger<ResetByAdminCommandHandler> logger) : ICommandHandler<ResetByAdminCommand>
{
    public async Task<Result> Handle(ResetByAdminCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.User.NotFound);

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);

        return Result.Success();
    }
}
