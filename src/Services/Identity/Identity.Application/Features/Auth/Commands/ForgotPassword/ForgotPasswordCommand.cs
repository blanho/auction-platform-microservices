namespace Identity.Application.Features.Auth.Commands.ForgotPassword;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record ForgotPasswordCommand(string Email) : ICommand;

public class ForgotPasswordCommandHandler(
    IUserService userService,
    IAuthHelper authHelper,
    IConfiguration configuration,
    ILogger<ForgotPasswordCommandHandler> logger) : ICommandHandler<ForgotPasswordCommand>
{
    private string RequiredFrontendUrl =>
        configuration["FrontendUrl"]
            ?? throw new InvalidOperationException("FrontendUrl configuration is required");

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email;
        var user = await userService.FindByEmailAsync(email);

        if (user == null || !user.EmailConfirmed)
        {
            logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", email);
            return Result.Success();
        }

        var token = await userService.GeneratePasswordResetTokenAsync(user);
        var resetLink = EmailLinkHelper.GeneratePasswordResetLink(RequiredFrontendUrl, user.Email!, token);

        await authHelper.PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, IdentityDefaults.EmailTemplate.PasswordReset, "Reset Your Password", new Dictionary<string, string>
        {
            [IdentityDefaults.EmailTemplate.UsernameKey] = user.UserName!,
            [IdentityDefaults.EmailTemplate.ResetLinkKey] = resetLink
        });

        logger.LogInformation("Password reset email requested for {Email}", email);
        return Result.Success();
    }
}
