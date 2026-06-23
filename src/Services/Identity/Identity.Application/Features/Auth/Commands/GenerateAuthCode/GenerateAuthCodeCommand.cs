namespace Identity.Application.Features.Auth.Commands.GenerateAuthCode;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record GenerateAuthCodeCommand(string UserId) : ICommand<string>;

public class GenerateAuthCodeCommandHandler(
    IUserService userService,
    ITokenGenerationService tokenService) : ICommandHandler<GenerateAuthCodeCommand, string>
{
    public async Task<Result<string>> Handle(GenerateAuthCodeCommand command, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure<string>(IdentityErrors.User.NotFound);

        var code = tokenService.GenerateTwoFactorStateToken(command.UserId);
        return Result.Success(code);
    }
}
