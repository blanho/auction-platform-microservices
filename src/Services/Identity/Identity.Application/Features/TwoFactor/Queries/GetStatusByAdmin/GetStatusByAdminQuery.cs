namespace Identity.Application.Features.TwoFactor.Queries.GetStatusByAdmin;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record GetStatusByAdminQuery(string UserId) : IQuery<TwoFactorStatusResponse>;

public class GetStatusByAdminQueryHandler(
    UserManager<ApplicationUser> userManager) : IQueryHandler<GetStatusByAdminQuery, TwoFactorStatusResponse>
{
    public async Task<Result<TwoFactorStatusResponse>> Handle(GetStatusByAdminQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.UserId);
        if (user == null)
            return Result.Failure<TwoFactorStatusResponse>(IdentityErrors.User.NotFound);

        var isEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
        var recoveryCodes = await userManager.CountRecoveryCodesAsync(user);

        return Result.Success(new TwoFactorStatusResponse
        {
            IsEnabled = isEnabled,
            HasAuthenticator = authenticatorKey != null,
            RecoveryCodesLeft = recoveryCodes,
            IsMachineRemembered = false
        });
    }
}
