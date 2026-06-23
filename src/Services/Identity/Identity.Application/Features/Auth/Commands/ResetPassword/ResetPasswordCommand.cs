namespace Identity.Application.Features.Auth.Commands.ResetPassword;

using System.Web;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record ResetPasswordCommand(ResetPasswordRequest Request) : ICommand;

public class ResetPasswordCommandHandler(
    IUserService userService,
    ILogger<ResetPasswordCommandHandler> logger) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await userService.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure(IdentityErrors.Auth.InvalidResetRequest);

        var decodedToken = HttpUtility.UrlDecode(request.Token);
        var result = await userService.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Password reset failed for {Email}: {Errors}", request.Email, string.Join(", ", errors));
            return Result.Failure(IdentityErrors.WithIdentityErrors("Auth.ResetFailed", "Password reset failed", errors));
        }

        logger.LogInformation("Password reset successful for {Email}", request.Email);
        return Result.Success();
    }
}
