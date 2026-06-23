namespace Identity.Application.Features.TwoFactor.Commands.SetupAuthenticator;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record SetupAuthenticatorCommand(string UserId) : ICommand<TwoFactorSetupResponse>;

public class SetupAuthenticatorCommandHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    System.Text.Encodings.Web.UrlEncoder urlEncoder,
    ILogger<SetupAuthenticatorCommandHandler> logger) : ICommandHandler<SetupAuthenticatorCommand, TwoFactorSetupResponse>
{
    public async Task<Result<TwoFactorSetupResponse>> Handle(SetupAuthenticatorCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure<TwoFactorSetupResponse>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        await userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(unformattedKey))
            return Result.Failure<TwoFactorSetupResponse>(Identity.Application.Errors.IdentityErrors.TwoFactor.SetupFailed);

        var sharedKey = Identity.Application.Helpers.TwoFactorHelper.FormatKey(unformattedKey);
        var authenticatorUri = Identity.Application.Helpers.TwoFactorHelper.GenerateQrCodeUri(urlEncoder, user.Email!, unformattedKey);

        return Result.Success(new TwoFactorSetupResponse
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeBase64 = string.Empty
        });
    }
}
