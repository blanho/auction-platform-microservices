namespace Identity.Application.Features.Profile.Commands.DisableTwoFactor;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Profile;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record DisableTwoFactorCommand(string UserId) : ICommand;

public class DisableTwoFactorCommandHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<DisableTwoFactorCommandHandler> logger) : ICommandHandler<DisableTwoFactorCommand>
{
    public async Task<Result> Handle(DisableTwoFactorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        await userManager.SetTwoFactorEnabledAsync(user, false);

        logger.LogInformation("User {UserId} disabled 2FA", command.UserId);

        return Result.Success();
    }
}
