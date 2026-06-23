namespace Identity.Application.Features.Auth.Commands.Logout;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record LogoutCommand(string UserId, string Token) : ICommand;

public class LogoutCommandHandler(
    ITokenGenerationService tokenService,
    ILogger<LogoutCommandHandler> logger) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var userId = command.UserId;
        var refreshToken = command.Token;
        var ipAddress = "system";

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            await tokenService.RevokeAllUserTokensAsync(userId, ipAddress);
        }

        logger.LogInformation("User {UserId} logged out successfully", userId);
        return Result.Success();
    }
}
