namespace Identity.Application.Features.TwoFactor.Commands.SetupAuthenticator;

using System.Text.Encodings.Web;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record SetupAuthenticatorCommand(string UserId) : ICommand<TwoFactorSetupResponse>;

public class SetupAuthenticatorCommandHandler(
    UserManager<ApplicationUser> userManager,
    UrlEncoder urlEncoder) : ICommandHandler<SetupAuthenticatorCommand, TwoFactorSetupResponse>
{
    public async Task<Result<TwoFactorSetupResponse>> Handle(SetupAuthenticatorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure<TwoFactorSetupResponse>(IdentityErrors.User.NotFound);

        await userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(unformattedKey))
            return Result.Failure<TwoFactorSetupResponse>(IdentityErrors.TwoFactor.SetupFailed);

        var sharedKey = TwoFactorHelper.FormatKey(unformattedKey);
        var authenticatorUri = TwoFactorHelper.GenerateQrCodeUri(urlEncoder, user.Email!, unformattedKey);

        return Result.Success(new TwoFactorSetupResponse
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeBase64 = string.Empty
        });
    }
}
