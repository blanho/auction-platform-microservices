namespace Identity.Application.Features.Auth.Commands.RevokeToken;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record RevokeTokenCommand(string Token, string IpAddress) : ICommand;

public class RevokeTokenCommandHandler(
    ITokenGenerationService tokenService) : ICommandHandler<RevokeTokenCommand>
{
    public async Task<Result> Handle(RevokeTokenCommand command, CancellationToken cancellationToken)
    {
        await tokenService.RevokeTokenAsync(command.Token, command.IpAddress);
        return Result.Success();
    }
}
