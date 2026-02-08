using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Identity.Api.DomainEvents;
using Identity.Api.DTOs.Audit;
using Identity.Api.DTOs.Auth;
using Identity.Api.DTOs.External;
using Identity.Api.DTOs.TwoFactor;
using Identity.Api.DTOs.Users;
using Identity.Api.Errors;
using Identity.Api.Helpers;
using Identity.Api.Interfaces;
using Identity.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Web;

namespace Identity.Api.Services;

public class AuthService(
    IUserService userService,
    SignInManager<ApplicationUser> signInManager,
    ITokenGenerationService tokenService,
    IConfiguration configuration,
    ILogger<AuthService> logger,
    IMediator mediator,
    IAuditPublisher auditPublisher,
    IMapper mapper) : IAuthService
{
    private const string ClientId = "nextApp";
    private const string UsernameKey = "username";

    public async Task<Result<UserDto>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await userService.FindByNameAsync(request.Username);
        if (existingUser != null)
            return Result.Failure<UserDto>(IdentityErrors.Auth.UsernameExists);

        existingUser = await userService.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result.Failure<UserDto>(IdentityErrors.Auth.EmailExists);

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = false
        };

        var result = await userService.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("User registration failed for {Username}: {Errors}", request.Username, string.Join(", ", errors));
            return Result.Failure<UserDto>(IdentityErrors.WithIdentityErrors("Auth.RegistrationFailed", "Registration failed", errors));
        }

        var roleResult = await userService.AddToRoleAsync(user, AppRoles.User);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Failed to assign default role to {Username}", request.Username);
        }

        var confirmationToken = await userService.GenerateEmailConfirmationTokenAsync(user);
        var frontendUrl = configuration["FrontendUrl"]
            ?? throw new InvalidOperationException("FrontendUrl configuration is required");
        var confirmationLink = EmailLinkHelper.GenerateConfirmationLink(frontendUrl, user.Id, confirmationToken);

        await mediator.Publish(new UserCreatedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = false,
            FullName = user.FullName,
            Role = AppRoles.User,
            ConfirmationLink = confirmationLink
        });

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, [AppRoles.User]),
            AuditAction.Created);

        logger.LogInformation("User {Username} registered successfully, awaiting email confirmation", request.Username);

        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles = [AppRoles.User];

        return Result.Success(userDto);
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
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

        await PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "welcome", "Welcome to Auction Platform", new Dictionary<string, string>
        {
            [UsernameKey] = user.UserName!
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

    public async Task<Result> ResendConfirmationAsync(string email)
    {
        var user = await userService.FindByEmailAsync(email);
        if (user == null)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(IdentityErrors.Auth.EmailAlreadyConfirmed);

        var token = await userService.GenerateEmailConfirmationTokenAsync(user);
        var frontendUrl = configuration["FrontendUrl"]
            ?? throw new InvalidOperationException("FrontendUrl configuration is required");
        var confirmationLink = EmailLinkHelper.GenerateConfirmationLink(frontendUrl, user.Id, token);

        await PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "email-confirmation", "Confirm Your Email", new Dictionary<string, string>
        {
            [UsernameKey] = user.UserName!,
            ["confirmationLink"] = confirmationLink
        });

        logger.LogInformation("Confirmation email resent to {Email}", email);

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await userService.FindByEmailAsync(email);

        if (user == null || !user.EmailConfirmed)
        {
            logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", email);
            return Result.Success();
        }

        var token = await userService.GeneratePasswordResetTokenAsync(user);
        var frontendUrl = configuration["FrontendUrl"]
            ?? throw new InvalidOperationException("FrontendUrl configuration is required");
        var resetLink = EmailLinkHelper.GeneratePasswordResetLink(frontendUrl, user.Email!, token);

        await PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "password-reset", "Reset Your Password", new Dictionary<string, string>
        {
            [UsernameKey] = user.UserName!,
            ["resetLink"] = resetLink
        });

        logger.LogInformation("Password reset email requested for {Email}", email);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
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

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await FindUserByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user == null)
        {
            logger.LogWarning("Login failed: user not found for {UsernameOrEmail}", request.UsernameOrEmail);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidCredentials);
        }

        if (user.IsSuspended)
        {
            logger.LogWarning("Login attempt for suspended user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.AccountSuspended(user.SuspensionReason));
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login attempt for unconfirmed email {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.EmailNotConfirmed);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("User {Username} is locked out", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.AccountLocked);
        }

        if (result.RequiresTwoFactor)
        {
            var twoFactorStateToken = tokenService.GenerateTwoFactorStateToken(user.Id);

            logger.LogInformation("User {Username} requires two-factor authentication", user.UserName);
            return Result.Success(new LoginResponse
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                RequiresTwoFactor = true,
                TwoFactorStateToken = twoFactorStateToken
            });
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed for user {Username}: invalid password", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidCredentials);
        }

        return await GenerateLoginResponseAsync(user, ipAddress);
    }

    public async Task<Result<LoginResponse>> LoginWith2FAAsync(TwoFactorLoginRequest request, string ipAddress)
    {
        var (isValid, userId) = tokenService.ValidateTwoFactorStateToken(request.TwoFactorStateToken);
        if (!isValid || string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Invalid or expired 2FA state token from {IpAddress}", ipAddress);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        var user = await userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<LoginResponse>(IdentityErrors.User.NotFound);

        var isValidCode = await userService.VerifyTwoFactorTokenAsync(
            user,
            signInManager.Options.Tokens.AuthenticatorTokenProvider,
            request.Code);

        if (!isValidCode)
        {
            logger.LogWarning("Invalid 2FA code for user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.TwoFactor.InvalidCode);
        }

        logger.LogInformation("User {Username} logged in successfully with 2FA", user.UserName);
        return await GenerateLoginResponseAsync(user, ipAddress);
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var result = await tokenService.RefreshTokenAsync(refreshToken, ClientId, ipAddress);

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

    public async Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        await tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        return Result.Success();
    }

    public async Task<Result> LogoutAsync(string userId, string refreshToken)
    {
        var ipAddress = "system";

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            await tokenService.RevokeAllUserTokensAsync(userId, ipAddress);
        }

        logger.LogInformation("User {UserId} logged out successfully", userId);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var result = await userService.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, string.Join(", ", errors));
            return Result.Failure(IdentityErrors.WithIdentityErrors("Profile.PasswordChangeFailed", "Password change failed", errors));
        }

        logger.LogInformation("Password changed successfully for user {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> LogoutAllAsync(string userId)
    {
        await tokenService.RevokeAllUserTokensAsync(userId, "logout-all");
        logger.LogInformation("All tokens revoked for user {UserId}", userId);
        return Result.Success();
    }

    public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync() =>
        await signInManager.GetExternalLoginInfoAsync();

    public async Task<Result<ExternalAuthResult>> ProcessExternalLoginAsync(ExternalLoginInfo info)
    {
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
            return Result.Failure<ExternalAuthResult>(IdentityErrors.External.EmailNotProvided);

        var user = await userService.FindByEmailAsync(email);

        if (user == null)
        {
            var username = await GenerateUniqueUsernameAsync(name ?? email.Split('@')[0]);
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

            await userService.AddToRoleAsync(user, AppRoles.User);

            await mediator.Publish(new UserCreatedDomainEvent
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = true,
                FullName = user.FullName,
                Role = AppRoles.User
            });

            await PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "welcome", "Welcome to Auction Platform", new Dictionary<string, string>
            {
                [UsernameKey] = user.UserName!
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

        var authCode = tokenService.GenerateTwoFactorStateToken(user.Id);
        logger.LogInformation("User {Username} logged in with {Provider}", user.UserName, info.LoginProvider);

        return Result.Success(new ExternalAuthResult(user.Id, authCode));
    }

    public async Task<Result<string>> GenerateAuthCodeAsync(string userId)
    {
        var user = await userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<string>(IdentityErrors.User.NotFound);

        var code = tokenService.GenerateTwoFactorStateToken(userId);
        return Result.Success(code);
    }

    public async Task<Result<LoginResponse>> ExchangeCodeForTokensAsync(string code, string ipAddress)
    {
        if (string.IsNullOrEmpty(code))
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);

        var (isValid, userId) = tokenService.ValidateTwoFactorStateToken(code);

        if (!isValid || string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Invalid or expired authorization code attempt");
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        var (user, _) = await userService.GetByIdWithRolesAsync(userId);
        if (user == null)
            return Result.Failure<LoginResponse>(IdentityErrors.User.NotFound);

        return await GenerateLoginResponseAsync(user, ipAddress);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        var user = await userService.FindByNameAsync(username);
        return user == null;
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var user = await userService.FindByEmailAsync(email);
        return user == null;
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(string userId)
    {
        var (user, roles) = await userService.GetByIdWithRolesAsync(userId);
        if (user == null)
            return Result.Failure<UserDto>(IdentityErrors.User.NotFound);

        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles = roles.ToList();
        return Result.Success(userDto);
    }

    private async Task<Result<LoginResponse>> GenerateLoginResponseAsync(ApplicationUser user, string ipAddress)
    {
        var tokens = await tokenService.GenerateTokenPairAsync(user.Id, ClientId, ipAddress);
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
            metadata: new Dictionary<string, object> { ["action"] = "login" });

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

    private async Task<string> GenerateUniqueUsernameAsync(string baseName)
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

    private async Task PublishEmailEventAsync(string userId, string email, string name, string templateKey, string subject, Dictionary<string, string> data)
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

    private async Task<ApplicationUser?> FindUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (MailAddress.TryCreate(usernameOrEmail, out _))
            return await userService.FindByEmailAsync(usernameOrEmail);

        return await userService.FindByNameAsync(usernameOrEmail);
    }
}