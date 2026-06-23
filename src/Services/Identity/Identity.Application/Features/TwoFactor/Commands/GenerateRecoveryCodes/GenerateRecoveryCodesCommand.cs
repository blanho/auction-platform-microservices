namespace Identity.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GenerateRecoveryCodesCommand(string UserId) : ICommand<RecoveryCodesResponse>;

public class GenerateRecoveryCodesCommandHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    ILogger<GenerateRecoveryCodesCommandHandler> logger) : ICommandHandler<GenerateRecoveryCodesCommand, RecoveryCodesResponse>
{
    public async Task<Result<RecoveryCodesResponse>> Handle(GenerateRecoveryCodesCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure<RecoveryCodesResponse>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return Result.Failure<RecoveryCodesResponse>(Identity.Application.Errors.IdentityErrors.TwoFactor.NotEnabled);

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return Result.Success(new RecoveryCodesResponse
        {
            RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>()
        });
    }
}
