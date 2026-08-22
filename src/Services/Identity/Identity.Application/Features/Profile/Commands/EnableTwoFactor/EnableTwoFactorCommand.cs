namespace Identity.Application.Features.Profile.Commands.EnableTwoFactor;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Profile;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record EnableTwoFactorCommand(string UserId) : ICommand;

public class EnableTwoFactorCommandHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<EnableTwoFactorCommandHandler> logger) : ICommandHandler<EnableTwoFactorCommand>
{
    public async Task<Result> Handle(EnableTwoFactorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        await userManager.SetTwoFactorEnabledAsync(user, true);

        logger.LogInformation("User {UserId} enabled 2FA", command.UserId);

        return Result.Success();
    }
}
