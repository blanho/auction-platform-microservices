namespace Identity.Application.Features.Auth.Commands.ResendConfirmation;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record ResendConfirmationCommand(string Email) : ICommand;

public class ResendConfirmationCommandHandler(
    IUserService userService,
    IAuthHelper authHelper,
    IConfiguration configuration,
    ILogger<ResendConfirmationCommandHandler> logger) : ICommandHandler<ResendConfirmationCommand>
{
    private string RequiredFrontendUrl =>
        configuration["FrontendUrl"]
            ?? throw new InvalidOperationException("FrontendUrl configuration is required");

    public async Task<Result> Handle(ResendConfirmationCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email;
        var user = await userService.FindByEmailAsync(email);
        if (user == null)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(IdentityErrors.Auth.EmailAlreadyConfirmed);

        var token = await userService.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = EmailLinkHelper.GenerateConfirmationLink(RequiredFrontendUrl, user.Id, token);

        await authHelper.PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "email-confirmation", "Confirm Your Email", new Dictionary<string, string>
        {
            [IdentityDefaults.EmailTemplate.UsernameKey] = user.UserName!,
            ["confirmationLink"] = confirmationLink
        });

        logger.LogInformation("Confirmation email resent to {Email}", email);

        return Result.Success();
    }
}
