namespace Identity.Application.Features.Auth.Commands.ExchangeCodeForTokens;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record ExchangeCodeForTokensCommand(string Code, string IpAddress) : ICommand<LoginResponse>;

public class ExchangeCodeForTokensCommandHandler(
    IUserService userService,
    ITokenGenerationService tokenService,
    IAuthHelper authHelper,
    ILogger<ExchangeCodeForTokensCommandHandler> logger) : ICommandHandler<ExchangeCodeForTokensCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(ExchangeCodeForTokensCommand command, CancellationToken cancellationToken)
    {
        var code = command.Code;
        var ipAddress = command.IpAddress;

        if (string.IsNullOrEmpty(code))
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);

        var (isValid, userId) = tokenService.ValidateTwoFactorStateToken(code);

        if (!isValid || string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Invalid or expired authorization code attempt");
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        var (user, _) = await userService.GetByIdWithRolesAsync(userId);
        if (user == null)
            return Result.Failure<LoginResponse>(IdentityErrors.User.NotFound);

        return await authHelper.GenerateLoginResponseAsync(user, ipAddress);
    }
}
