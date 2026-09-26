namespace Identity.Application.Features.TwoFactor.Queries.GetStatus;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record GetStatusQuery(string UserId) : IQuery<TwoFactorStatusResponse>;

public class GetStatusQueryHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IQueryHandler<GetStatusQuery, TwoFactorStatusResponse>
{
    public async Task<Result<TwoFactorStatusResponse>> Handle(GetStatusQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.UserId);
        if (user == null)
            return Result.Failure<TwoFactorStatusResponse>(IdentityErrors.User.NotFound);

        var isEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
        var recoveryCodes = await userManager.CountRecoveryCodesAsync(user);
        var isMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);

        return Result.Success(new TwoFactorStatusResponse
        {
            IsEnabled = isEnabled,
            HasAuthenticator = authenticatorKey != null,
            RecoveryCodesLeft = recoveryCodes,
            IsMachineRemembered = isMachineRemembered
        });
    }
}
