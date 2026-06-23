namespace Identity.Application.Features.Auth.Commands.ConfirmEmail;

using System.Web;
using Identity.Application.DTOs.Auth;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Identity.Domain.Events;
using Identity.Application.Errors;

public record ConfirmEmailCommand(ConfirmEmailRequest Request) : ICommand;

public class ConfirmEmailCommandHandler(
    IUserService userService,
    IMediator mediator,
    IAuthHelper authHelper,
    ILogger<ConfirmEmailCommandHandler> logger) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<Result> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await userService.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.Auth.InvalidConfirmationLink);

        if (user.EmailConfirmed)
            return Result.Success();

        var decodedToken = HttpUtility.UrlDecode(request.Token);
        var result = await userService.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Email confirmation failed for {UserId}: {Errors}", request.UserId, string.Join(", ", errors));
            return Result.Failure(IdentityErrors.WithIdentityErrors("Auth.ConfirmationFailed", "Email confirmation failed", errors));
        }

        await authHelper.PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "welcome", "Welcome to Auction Platform", new Dictionary<string, string>
        {
            [IdentityDefaults.EmailTemplate.UsernameKey] = user.UserName!
        });

        await mediator.Publish(new UserEmailConfirmedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        });

        logger.LogInformation("Email confirmed for user {Username}", user.UserName);
        return Result.Success();
    }
}
