namespace Identity.Application.Features.Auth.Commands.LoginWith2FA;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record LoginWith2FACommand(TwoFactorLoginRequest Request, string IpAddress) : ICommand<LoginResponse>;

public class LoginWith2FACommandHandler(
    IUserService userService,
    SignInManager<ApplicationUser> signInManager,
    ITokenGenerationService tokenService,
    IAuthHelper authHelper,
    ILogger<LoginWith2FACommandHandler> logger) : ICommandHandler<LoginWith2FACommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginWith2FACommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var ipAddress = command.IpAddress;

        var (isValid, userId) = tokenService.ValidateTwoFactorStateToken(request.TwoFactorStateToken);
        if (!isValid || string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Invalid or expired 2FA state token from {IpAddress}", ipAddress);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        var user = await userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<LoginResponse>(IdentityErrors.User.NotFound);

        var isValidCode = await userService.VerifyTwoFactorTokenAsync(
            user,
            signInManager.Options.Tokens.AuthenticatorTokenProvider,
            request.Code);

        if (!isValidCode)
        {
            logger.LogWarning("Invalid 2FA code for user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.TwoFactor.InvalidCode);
        }

        logger.LogInformation("User {Username} logged in successfully with 2FA", user.UserName);
        return await authHelper.GenerateLoginResponseAsync(user, ipAddress);
    }
}
