namespace Identity.Application.Features.Auth.Commands.LogoutAll;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record LogoutAllCommand(string UserId) : ICommand;

public class LogoutAllCommandHandler(
    ITokenGenerationService tokenService,
    ILogger<LogoutAllCommandHandler> logger) : ICommandHandler<LogoutAllCommand>
{
    public async Task<Result> Handle(LogoutAllCommand command, CancellationToken cancellationToken)
    {
        await tokenService.RevokeAllUserTokensAsync(command.UserId, "logout-all");
        logger.LogInformation("All tokens revoked for user {UserId}", command.UserId);
        return Result.Success();
    }
}
