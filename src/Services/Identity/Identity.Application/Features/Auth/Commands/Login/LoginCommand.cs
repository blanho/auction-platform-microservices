namespace Identity.Application.Features.Auth.Commands.Login;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record LoginCommand(LoginRequest Request, string IpAddress) : ICommand<LoginResponse>;

public class LoginCommandHandler(
    SignInManager<ApplicationUser> signInManager,
    ITokenGenerationService tokenService,
    IAuthHelper authHelper,
    ILogger<LoginCommandHandler> logger) : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await authHelper.FindUserByUsernameOrEmailAsync(command.Request.UsernameOrEmail);
        if (user == null)
        {
            logger.LogWarning("Login failed: user not found for {UsernameOrEmail}", command.Request.UsernameOrEmail);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidCredentials);
        }

        if (user.IsSuspended)
        {
            logger.LogWarning("Login attempt for suspended user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.AccountSuspended(user.SuspensionReason));
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login attempt for unconfirmed email {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.EmailNotConfirmed);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("User {Username} is locked out", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.AccountLocked);
        }

        if (result.RequiresTwoFactor)
        {
            var twoFactorStateToken = tokenService.GenerateTwoFactorStateToken(user.Id);

            logger.LogInformation("User {Username} requires two-factor authentication", user.UserName);
            return Result.Success(new LoginResponse
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                RequiresTwoFactor = true,
                TwoFactorStateToken = twoFactorStateToken
            });
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed for user {Username}: invalid password", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidCredentials);
        }

        return await authHelper.GenerateLoginResponseAsync(user, command.IpAddress);
    }
}
