namespace Identity.Application.Features.Auth.Commands.ProcessExternalLogin;

using System.Security.Claims;
using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record ProcessExternalLoginCommand(ExternalLoginInfo Info) : ICommand<ExternalAuthResult>;

public class ProcessExternalLoginCommandHandler(
    IUserService userService,
    IAuthorizationCodeStore authorizationCodeStore,
    IAuthHelper authHelper,
    IMediator mediator,
    ILogger<ProcessExternalLoginCommandHandler> logger) : ICommandHandler<ProcessExternalLoginCommand, ExternalAuthResult>
{
    public async Task<Result<ExternalAuthResult>> Handle(ProcessExternalLoginCommand command, CancellationToken cancellationToken)
    {
        var info = command.Info;
        var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
            return Result.Failure<ExternalAuthResult>(IdentityErrors.External.EmailNotProvided);

        var user = await userService.FindByEmailAsync(email);

        if (user == null)
        {
            var username = await authHelper.GenerateUniqueUsernameAsync(name ?? email.Split('@')[0]);
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FullName = name
            };

            var createResult = await userService.CreateWithoutPasswordAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                logger.LogError("Failed to create user from external login: {Errors}", string.Join(", ", errors));
                return Result.Failure<ExternalAuthResult>(IdentityErrors.WithIdentityErrors("Auth.RegistrationFailed", "Failed to create account", errors));
            }

            await userService.EnsureRoleExistsAsync(Roles.User);
            await userService.AddToRoleAsync(user, Roles.User);

            await mediator.Publish(new UserCreatedDomainEvent
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = true,
                FullName = user.FullName,
                Role = Roles.User
            });

            await authHelper.PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "welcome", "Welcome to Auction Platform", new Dictionary<string, string>
            {
                [IdentityDefaults.EmailTemplate.UsernameKey] = user.UserName!
            });
        }
        else if (user.IsSuspended)
        {
            return Result.Failure<ExternalAuthResult>(IdentityErrors.Auth.AccountSuspended(user.SuspensionReason));
        }

        var existingLogins = await userService.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
        {
            var addLoginResult = await userService.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                logger.LogWarning("Failed to add external login for user {UserId}", user.Id);
            }
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userService.UpdateAsync(user);

        var authCode = await authorizationCodeStore.CreateAsync(user.Id, cancellationToken);
        logger.LogInformation("User {Username} logged in with {Provider}", user.UserName, info.LoginProvider);

        return Result.Success(new ExternalAuthResult(user.Id, authCode));
    }
}
