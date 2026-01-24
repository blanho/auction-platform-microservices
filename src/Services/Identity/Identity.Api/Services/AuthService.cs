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

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenGenerationService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IMediator _mediator;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IMapper _mapper;
    private const string ClientId = "nextApp";

    public AuthService(
        IUserService userService,
        SignInManager<ApplicationUser> signInManager,
        ITokenGenerationService tokenService,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IMediator mediator,
        IAuditPublisher auditPublisher,
        IMapper mapper)
    {
        _userService = userService;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
        _auditPublisher = auditPublisher;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterRequest dto)
    {
        var existingUser = await _userService.FindByNameAsync(dto.Username);
        if (existingUser != null)
            return Result.Failure<UserDto>(IdentityErrors.Auth.UsernameExists);

        existingUser = await _userService.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Result.Failure<UserDto>(IdentityErrors.Auth.EmailExists);

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            EmailConfirmed = false
        };

        var result = await _userService.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("User registration failed for {Username}: {Errors}", dto.Username, string.Join(", ", errors));
            return Result.Failure<UserDto>(IdentityErrors.WithIdentityErrors("Auth.RegistrationFailed", "Registration failed", errors));
        }

        var roleResult = await _userService.AddToRoleAsync(user, AppRoles.User);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign default role to {Username}", dto.Username);
        }

        var confirmationToken = await _userService.GenerateEmailConfirmationTokenAsync(user);
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var confirmationLink = EmailLinkHelper.GenerateConfirmationLink(frontendUrl, user.Id, confirmationToken);

        await _mediator.Publish(new UserCreatedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = false,
            FullName = user.FullName,
            Role = AppRoles.User,
            ConfirmationLink = confirmationLink
        });

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, [AppRoles.User]),
            AuditAction.Created);

        _logger.LogInformation("User {Username} registered successfully, awaiting email confirmation", dto.Username);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = [AppRoles.User];

        return Result.Success(userDto);
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var user = await _userService.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.Auth.InvalidConfirmationLink);

        if (user.EmailConfirmed)
            return Result.Success();

        var decodedToken = HttpUtility.UrlDecode(request.Token);
        var result = await _userService.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Email confirmation failed for {UserId}: {Errors}", request.UserId, string.Join(", ", errors));
            return Result.Failure(IdentityErrors.WithIdentityErrors("Auth.ConfirmationFailed", "Email confirmation failed", errors));
        }

        await PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "welcome", "Welcome to Auction Platform", new Dictionary<string, string>
        {
            ["username"] = user.UserName!
        });

        await _mediator.Publish(new UserEmailConfirmedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        });

        _logger.LogInformation("Email confirmed for user {Username}", user.UserName);
        return Result.Success();
    }

    public async Task<Result> ResendConfirmationAsync(string email)
    {
        var user = await _userService.FindByEmailAsync(email);
        if (user == null)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(IdentityErrors.Auth.EmailAlreadyConfirmed);

        var token = await _userService.GenerateEmailConfirmationTokenAsync(user);
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var confirmationLink = EmailLinkHelper.GenerateConfirmationLink(frontendUrl, user.Id, token);

        await PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "email-confirmation", "Confirm Your Email", new Dictionary<string, string>
        {
            ["username"] = user.UserName!,
            ["confirmationLink"] = confirmationLink
        });

        _logger.LogInformation("Confirmation email resent to {Email}", email);

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await _userService.FindByEmailAsync(email);

        if (user == null || !user.EmailConfirmed)
        {
            _logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", email);
            return Result.Success();
        }

        var token = await _userService.GeneratePasswordResetTokenAsync(user);
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var resetLink = EmailLinkHelper.GeneratePasswordResetLink(frontendUrl, user.Email!, token);

        await PublishEmailEventAsync(user.Id, user.Email!, user.UserName!, "password-reset", "Reset Your Password", new Dictionary<string, string>
        {
            ["username"] = user.UserName!,
            ["resetLink"] = resetLink
        });

        _logger.LogInformation("Password reset email requested for {Email}", email);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userService.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure(IdentityErrors.Auth.InvalidResetRequest);

        var decodedToken = HttpUtility.UrlDecode(request.Token);
        var result = await _userService.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Password reset failed for {Email}: {Errors}", request.Email, string.Join(", ", errors));
            return Result.Failure(IdentityErrors.WithIdentityErrors("Auth.ResetFailed", "Password reset failed", errors));
        }

        _logger.LogInformation("Password reset successful for {Email}", request.Email);
        return Result.Success();
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest dto, string ipAddress)
    {
        var user = await FindUserByUsernameOrEmailAsync(dto.UsernameOrEmail);
        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found for {UsernameOrEmail}", dto.UsernameOrEmail);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidCredentials);
        }

        if (user.IsSuspended)
        {
            _logger.LogWarning("Login attempt for suspended user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.AccountSuspended(user.SuspensionReason));
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login attempt for unconfirmed email {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.EmailNotConfirmed);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Username} is locked out", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.AccountLocked);
        }

        if (result.RequiresTwoFactor)
        {
            var twoFactorStateToken = _tokenService.GenerateTwoFactorStateToken(user.Id);

            _logger.LogInformation("User {Username} requires two-factor authentication", user.UserName);
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
            _logger.LogWarning("Login failed for user {Username}: invalid password", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidCredentials);
        }

        return await GenerateLoginResponseAsync(user, ipAddress);
    }

    public async Task<Result<LoginResponse>> LoginWith2FAAsync(TwoFactorLoginRequest request, string ipAddress)
    {
        var (isValid, userId) = _tokenService.ValidateTwoFactorStateToken(request.TwoFactorStateToken);
        if (!isValid || string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid or expired 2FA state token from {IpAddress}", ipAddress);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        var user = await _userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<LoginResponse>(IdentityErrors.User.NotFound);

        var isValidCode = await _userService.VerifyTwoFactorTokenAsync(
            user,
            _signInManager.Options.Tokens.AuthenticatorTokenProvider,
            request.Code);

        if (!isValidCode)
        {
            _logger.LogWarning("Invalid 2FA code for user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.TwoFactor.InvalidCode);
        }

        _logger.LogInformation("User {Username} logged in successfully with 2FA", user.UserName);
        return await GenerateLoginResponseAsync(user, ipAddress);
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var tokens = await _tokenService.RefreshTokenAsync(refreshToken, ClientId, ipAddress);

        if (tokens == null)
        {
            _logger.LogWarning("Invalid refresh token attempt from {IpAddress}", ipAddress);
            return Result.Failure<TokenResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        _logger.LogInformation("Token refreshed successfully from {IpAddress}", ipAddress);

        return Result.Success(new TokenResponse(
            tokens.Value.AccessToken,
            tokens.Value.RefreshToken,
            tokens.Value.ExpiresIn
        ));
    }

    public async Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        await _tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        return Result.Success();
    }

    public async Task<Result> LogoutAsync(string userId, string refreshToken)
    {
        var ipAddress = "system";

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            await _tokenService.RevokeAllUserTokensAsync(userId, ipAddress);
        }

        _logger.LogInformation("User {UserId} logged out successfully", userId);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var result = await _userService.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, string.Join(", ", errors));
            return Result.Failure(IdentityErrors.WithIdentityErrors("Profile.PasswordChangeFailed", "Password change failed", errors));
        }

        _logger.LogInformation("Password changed successfully for user {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> LogoutAllAsync(string userId)
    {
        await _tokenService.RevokeAllUserTokensAsync(userId, "logout-all");
        _logger.LogInformation("All tokens revoked for user {UserId}", userId);
        return Result.Success();
    }

    public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync() =>
        await _signInManager.GetExternalLoginInfoAsync();

    public async Task<Result<ExternalAuthResult>> ProcessExternalLoginAsync(ExternalLoginInfo info)
    {
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
            return Result.Failure<ExternalAuthResult>(IdentityErrors.External.EmailNotProvided);

        var user = await _userService.FindByEmailAsync(email);

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

            var createResult = await _userService.CreateWithoutPasswordAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                _logger.LogError("Failed to create user from external login: {Errors}", string.Join(", ", errors));
                return Result.Failure<ExternalAuthResult>(IdentityErrors.WithIdentityErrors("Auth.RegistrationFailed", "Failed to create account", errors));
            }

            await _userService.AddToRoleAsync(user, AppRoles.User);

            await _mediator.Publish(new UserCreatedDomainEvent
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
                ["username"] = user.UserName!
            });
        }
        else if (user.IsSuspended)
        {
            return Result.Failure<ExternalAuthResult>(IdentityErrors.Auth.AccountSuspended(user.SuspensionReason));
        }

        var existingLogins = await _userService.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
        {
            var addLoginResult = await _userService.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                _logger.LogWarning("Failed to add external login for user {UserId}", user.Id);
            }
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userService.UpdateAsync(user);

        var authCode = _tokenService.GenerateTwoFactorStateToken(user.Id);
        _logger.LogInformation("User {Username} logged in with {Provider}", user.UserName, info.LoginProvider);

        return Result.Success(new ExternalAuthResult(user.Id, authCode));
    }

    public async Task<Result<string>> GenerateAuthCodeAsync(string userId)
    {
        var user = await _userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<string>(IdentityErrors.User.NotFound);

        var code = _tokenService.GenerateTwoFactorStateToken(userId);
        return Result.Success(code);
    }

    public async Task<Result<LoginResponse>> ExchangeCodeForTokensAsync(string code, string ipAddress)
    {
        if (string.IsNullOrEmpty(code))
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);

        var (isValid, userId) = _tokenService.ValidateTwoFactorStateToken(code);

        if (!isValid || string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid or expired authorization code attempt");
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        var (user, _) = await _userService.GetByIdWithRolesAsync(userId);
        if (user == null)
            return Result.Failure<LoginResponse>(IdentityErrors.User.NotFound);

        return await GenerateLoginResponseAsync(user, ipAddress);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        var user = await _userService.FindByNameAsync(username);
        return user == null;
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var user = await _userService.FindByEmailAsync(email);
        return user == null;
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(string userId)
    {
        var (user, roles) = await _userService.GetByIdWithRolesAsync(userId);
        if (user == null)
            return Result.Failure<UserDto>(IdentityErrors.User.NotFound);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles.ToList();
        return Result.Success(userDto);
    }

    private async Task<Result<LoginResponse>> GenerateLoginResponseAsync(ApplicationUser user, string ipAddress)
    {
        var tokens = await _tokenService.GenerateTokenPairAsync(user.Id, ClientId, ipAddress);
        if (tokens == null)
        {
            _logger.LogError("Failed to generate tokens for user {Username}", user.UserName);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidRefreshToken);
        }

        var updateTask = Task.Run(async () =>
        {
            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _userService.UpdateAsync(user);
        });
        var rolesTask = _userService.GetRolesAsync(user);

        await Task.WhenAll(updateTask, rolesTask);
        var roles = rolesTask.Result;

        await _auditPublisher.PublishAsync(
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

        _logger.LogInformation("User {Username} logged in successfully", user.UserName);

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

        while (await _userService.FindByNameAsync(username) != null)
            username = $"{sanitized}{counter++}";

        return username;
    }

    private async Task PublishEmailEventAsync(string userId, string email, string name, string templateKey, string subject, Dictionary<string, string> data)
    {
        await _mediator.Publish(new EmailNotificationRequestedDomainEvent
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
            return await _userService.FindByEmailAsync(usernameOrEmail);

        return await _userService.FindByNameAsync(usernameOrEmail);
    }
}