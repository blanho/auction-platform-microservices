namespace Identity.Application.Features.TwoFactor.Queries.GetStatusByAdmin;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetStatusByAdminQuery(string UserId) : IQuery<TwoFactorStatusResponse>;

public class GetStatusByAdminQueryHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    ILogger<GetStatusByAdminQueryHandler> logger) : IQueryHandler<GetStatusByAdminQuery, TwoFactorStatusResponse>
{
    public async Task<Result<TwoFactorStatusResponse>> Handle(GetStatusByAdminQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.UserId);
        if (user == null)
            return Result.Failure<TwoFactorStatusResponse>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var isEnabledTask = userManager.GetTwoFactorEnabledAsync(user);
        var authenticatorKeyTask = userManager.GetAuthenticatorKeyAsync(user);
        var recoveryCodesTask = userManager.CountRecoveryCodesAsync(user);

        await Task.WhenAll(isEnabledTask, authenticatorKeyTask, recoveryCodesTask);
        var isEnabled = await isEnabledTask;
        var authenticatorKey = await authenticatorKeyTask;
        var recoveryCodes = await recoveryCodesTask;

        return Result.Success(new TwoFactorStatusResponse
        {
            IsEnabled = isEnabled,
            HasAuthenticator = authenticatorKey != null,
            RecoveryCodesLeft = recoveryCodes,
            IsMachineRemembered = false
        });
    }
}
