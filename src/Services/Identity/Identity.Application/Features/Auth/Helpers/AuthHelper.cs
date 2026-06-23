namespace Identity.Application.Features.Auth.Helpers;

using BuildingBlocks.Application.Abstractions.Auditing;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Events;
using MediatR;
using System.Net.Mail;
using Identity.Application.DTOs.Audit;
using Microsoft.Extensions.Logging;

public class AuthHelper(
    IUserService userService,
    ITokenGenerationService tokenService,
    ILogger<AuthHelper> logger,
    IMediator mediator,
    IAuditPublisher auditPublisher) : IAuthHelper
{
    public async Task<Result<LoginResponse>> GenerateLoginResponseAsync(ApplicationUser user, string ipAddress)
    {
        var tokens = await tokenService.GenerateTokenPairAsync(user.Id, IdentityDefaults.OAuth.DefaultClientId, ipAddress);
        if (tokens == null)
        {
            logger.LogError("Failed to generate tokens for user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userService.UpdateAsync(user);

        var roles = await userService.GetRolesAsync(user);

        await mediator.Publish(new UserLoginDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            IpAddress = ipAddress
        });

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new AuthAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = "Login",
                IpAddress = ipAddress,
                Success = true
            },
            AuditAction.Updated,
            metadata: new Dictionary<string, object> { [AuditMetadataKeys.ActionLower] = IdentityDefaults.Audit.Login });

        logger.LogInformation("User {Username} logged in successfully", user.UserName);

        return Result.Success(new LoginResponse
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList(),
            AccessToken = tokens.Value.AccessToken,
            RefreshToken = tokens.Value.RefreshToken,
            ExpiresIn = tokens.Value.ExpiresIn,
            RequiresTwoFactor = false
        });
    }

    public async Task<string> GenerateUniqueUsernameAsync(string baseName)
    {
        var sanitized = new string(baseName.Where(c => char.IsLetterOrDigit(c)).ToArray());
        if (sanitized.Length < 3)
            sanitized = "user";

        var username = sanitized;
        var counter = 1;

        while (await userService.FindByNameAsync(username) != null)
            username = $"{sanitized}{counter++}";

        return username;
    }

    public async Task PublishEmailEventAsync(string userId, string email, string name, string templateKey, string subject, Dictionary<string, string> data)
    {
        await mediator.Publish(new EmailNotificationRequestedDomainEvent
        {
            UserId = userId,
            RecipientEmail = email,
            RecipientName = name,
            TemplateKey = templateKey,
            Subject = subject,
            Data = data
        });
    }

    public async Task<ApplicationUser?> FindUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (MailAddress.TryCreate(usernameOrEmail, out _))
            return await userService.FindByEmailAsync(usernameOrEmail);

        return await userService.FindByNameAsync(usernameOrEmail);
    }
}
