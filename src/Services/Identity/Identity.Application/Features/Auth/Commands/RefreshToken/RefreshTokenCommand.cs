namespace Identity.Application.Features.Auth.Commands.RefreshToken;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record RefreshTokenCommand(string Token, string IpAddress) : ICommand<TokenResponse>;

public class RefreshTokenCommandHandler(
    ITokenGenerationService tokenService,
    ILogger<RefreshTokenCommandHandler> logger) : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var refreshToken = command.Token;
        var ipAddress = command.IpAddress;

        var result = await tokenService.RefreshTokenAsync(refreshToken, IdentityDefaults.OAuth.DefaultClientId, ipAddress);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Invalid refresh token attempt from {IpAddress}, reason: {Reason}",
                ipAddress, result.FailureReason);

            return result.FailureReason == RefreshTokenFailureReason.SecurityTermination
                ? Result.Failure<TokenResponse>(IdentityErrors.Auth.SecurityTermination)
                : Result.Failure<TokenResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        logger.LogInformation("Token refreshed successfully from {IpAddress}", ipAddress);

        return Result.Success(new TokenResponse(
            result.AccessToken!,
            result.RefreshToken,
            result.ExpiresIn
        ));
    }
}
