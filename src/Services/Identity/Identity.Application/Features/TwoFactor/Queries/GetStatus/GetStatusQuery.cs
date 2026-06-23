namespace Identity.Application.Features.TwoFactor.Queries.GetStatus;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetStatusQuery(string UserId) : IQuery<TwoFactorStatusResponse>;

public class GetStatusQueryHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    Microsoft.AspNetCore.Identity.SignInManager<Identity.Domain.Entities.ApplicationUser> signInManager,
    ILogger<GetStatusQueryHandler> logger) : IQueryHandler<GetStatusQuery, TwoFactorStatusResponse>
{
    public async Task<Result<TwoFactorStatusResponse>> Handle(GetStatusQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.UserId);
        if (user == null)
            return Result.Failure<TwoFactorStatusResponse>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var isEnabledTask = userManager.GetTwoFactorEnabledAsync(user);
        var authenticatorKeyTask = userManager.GetAuthenticatorKeyAsync(user);
        var recoveryCodesTask = userManager.CountRecoveryCodesAsync(user);
        var isMachineRememberedTask = signInManager.IsTwoFactorClientRememberedAsync(user);

        await Task.WhenAll(isEnabledTask, authenticatorKeyTask, recoveryCodesTask, isMachineRememberedTask);
        var isEnabled = await isEnabledTask;
        var authenticatorKey = await authenticatorKeyTask;
        var recoveryCodes = await recoveryCodesTask;
        var isMachineRemembered = await isMachineRememberedTask;

        return Result.Success(new TwoFactorStatusResponse
        {
            IsEnabled = isEnabled,
            HasAuthenticator = authenticatorKey != null,
            RecoveryCodesLeft = recoveryCodes,
            IsMachineRemembered = isMachineRemembered
        });
    }
}
