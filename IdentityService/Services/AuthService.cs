using Common.Core.Authorization;
using IdentityService.DTOs;
using IdentityService.Interfaces;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System.Web;

namespace IdentityService.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenGenerationService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private const string ClientId = "nextApp";

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenGenerationService tokenService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult<UserDto>> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByNameAsync(dto.Username);
        if (existingUser != null)
            return AuthResult<UserDto>.Failure("Username already exists");

        existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return AuthResult<UserDto>.Failure("Email already registered");

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("User registration failed for {Username}: {Errors}", dto.Username, string.Join(", ", errors));
            return AuthResult<UserDto>.Failure("Registration failed", errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.User);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign default role to {Username}", dto.Username);
        }

        await SendConfirmationEmailAsync(user);

        _logger.LogInformation("User {Username} registered successfully, awaiting email confirmation", dto.Username);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = AppRoles.User
        };

        return AuthResult<UserDto>.Success(userDto, "Registration successful. Please check your email to confirm your account.");
    }

    public async Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return AuthResult.Failure("Invalid confirmation link");

        if (user.EmailConfirmed)
            return AuthResult.Success("Email already confirmed");

        var decodedToken = HttpUtility.UrlDecode(dto.Token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Email confirmation failed for {UserId}: {Errors}", dto.UserId, string.Join(", ", errors));
            return AuthResult.Failure("Email confirmation failed. The link may have expired.", errors);
        }

        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        _logger.LogInformation("Email confirmed for user {Username}", user.UserName);
        return AuthResult.Success("Email confirmed successfully. You can now log in.");
    }

    public async Task<AuthResult> ResendConfirmationAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return AuthResult.Success("If an account exists with this email, a confirmation link will be sent.");

        if (user.EmailConfirmed)
            return AuthResult.Failure("Email is already confirmed");

        try
        {
            await SendConfirmationEmailAsync(user);
            _logger.LogInformation("Confirmation email resent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend confirmation email to {Email}", email);
            return AuthResult.Failure("Failed to send email. Please try again later.");
        }

        return AuthResult.Success("Confirmation email sent. Please check your inbox.");
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !user.EmailConfirmed)
        {
            _logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", email);
            return AuthResult.Success("If an account exists with this email, a password reset link will be sent.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/auth/reset-password?email={HttpUtility.UrlEncode(user.Email!)}&token={encodedToken}";

        try
        {
            await _emailService.SendPasswordResetAsync(user.Email!, user.UserName!, resetLink);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
        }

        return AuthResult.Success("If an account exists with this email, a password reset link will be sent.");
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return AuthResult.Failure("Invalid reset request");

        var decodedToken = HttpUtility.UrlDecode(dto.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Password reset failed for {Email}: {Errors}", dto.Email, string.Join(", ", errors));
            return AuthResult.Failure("Password reset failed. The link may have expired.", errors);
        }

        _logger.LogInformation("Password reset successful for {Email}", dto.Email);
        return AuthResult.Success("Password reset successful. You can now log in with your new password.");
    }

    public async Task<AuthResult<LoginResponseDto>> LoginAsync(LoginDto dto, string ipAddress)
    {
        var user = await FindUserByUsernameOrEmailAsync(dto.UsernameOrEmail);
        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found for {UsernameOrEmail}", dto.UsernameOrEmail);
            return AuthResult<LoginResponseDto>.Failure("Invalid credentials");
        }

        if (user.IsSuspended)
        {
            _logger.LogWarning("Login attempt for suspended user {Username}", user.UserName);
            return AuthResult<LoginResponseDto>.Failure("Account is suspended", 
                new List<string> { user.SuspensionReason ?? "Contact support for more information" });
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login attempt for unconfirmed email {Username}", user.UserName);
            return AuthResult<LoginResponseDto>.Failure("Please confirm your email before logging in");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Username} is locked out", user.UserName);
            return AuthResult<LoginResponseDto>.Failure("Account is locked. Please try again later.");
        }

        if (result.RequiresTwoFactor)
        {
            var twoFactorStateToken = _tokenService.GenerateTwoFactorStateToken(user.Id);
            
            _logger.LogInformation("User {Username} requires two-factor authentication", user.UserName);
            return AuthResult<LoginResponseDto>.TwoFactorRequired(new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                RequiresTwoFactor = true,
                TwoFactorStateToken = twoFactorStateToken // Secure token instead of just UserId
            }, "Two-factor authentication required");
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed for user {Username}: invalid password", user.UserName);
            return AuthResult<LoginResponseDto>.Failure("Invalid credentials");
        }

        var tokens = await _tokenService.GenerateTokenPairAsync(user.Id, ClientId, ipAddress);
        if (tokens == null)
        {
            _logger.LogError("Failed to generate tokens for user {Username}", user.UserName);
            return AuthResult<LoginResponseDto>.Failure("Failed to generate authentication tokens");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "user";

        _logger.LogInformation("User {Username} logged in successfully", user.UserName);

        return AuthResult<LoginResponseDto>.Success(new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = role,
            AccessToken = tokens.Value.AccessToken,
            RefreshToken = tokens.Value.RefreshToken,
            ExpiresIn = tokens.Value.ExpiresIn,
            RequiresTwoFactor = false
        }, "Login successful");
    }

    public async Task<AuthResult<LoginResponseDto>> LoginWith2FAAsync(TwoFactorLoginDto dto, string ipAddress)
    {
        var (isValid, userId) = _tokenService.ValidateTwoFactorStateToken(dto.TwoFactorStateToken);
        if (!isValid || string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid or expired 2FA state token from {IpAddress}", ipAddress);
            return AuthResult<LoginResponseDto>.Failure("Invalid or expired authentication state. Please login again.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AuthResult<LoginResponseDto>.Failure("Invalid authentication attempt");

        var isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            dto.Code);

        if (!isValidCode)
        {
            _logger.LogWarning("Invalid 2FA code for user {Username}", user.UserName);
            return AuthResult<LoginResponseDto>.Failure("Invalid verification code");
        }

        var tokens = await _tokenService.GenerateTokenPairAsync(user.Id, ClientId, ipAddress);
        if (tokens == null)
            return AuthResult<LoginResponseDto>.Failure("Failed to generate authentication tokens");

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "user";

        _logger.LogInformation("User {Username} logged in successfully with 2FA", user.UserName);

        return AuthResult<LoginResponseDto>.Success(new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = role,
            AccessToken = tokens.Value.AccessToken,
            RefreshToken = tokens.Value.RefreshToken,
            ExpiresIn = tokens.Value.ExpiresIn,
            RequiresTwoFactor = false
        }, "Login successful");
    }

    public async Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var tokens = await _tokenService.RefreshTokenAsync(refreshToken, ClientId, ipAddress);

        if (tokens == null)
        {
            _logger.LogWarning("Invalid refresh token attempt from {IpAddress}", ipAddress);
            return AuthResult<TokenResponseDto>.Failure("Invalid or expired refresh token");
        }

        _logger.LogInformation("Token refreshed successfully from {IpAddress}", ipAddress);

        return AuthResult<TokenResponseDto>.Success(new TokenResponseDto
        {
            AccessToken = tokens.Value.AccessToken,
            RefreshToken = tokens.Value.RefreshToken,
            ExpiresIn = tokens.Value.ExpiresIn
        }, "Token refreshed successfully");
    }

    public async Task<AuthResult> RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        await _tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        return AuthResult.Success("Token revoked successfully");
    }

    public async Task<AuthResult> LogoutAsync(string userId, string refreshToken)
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
        return AuthResult.Success("Logged out successfully");
    }

    public async Task<AuthResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AuthResult.Failure("User not found");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, string.Join(", ", errors));
            return AuthResult.Failure("Password change failed", errors);
        }

        _logger.LogInformation("Password changed successfully for user {UserId}", userId);
        return AuthResult.Success("Password changed successfully");
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var confirmationLink = $"{frontendUrl}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailConfirmationAsync(user.Email!, user.UserName!, confirmationLink);
        _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
    }

    private async Task<ApplicationUser?> FindUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (usernameOrEmail.Contains('@'))
            return await _userManager.FindByEmailAsync(usernameOrEmail);
        return await _userManager.FindByNameAsync(usernameOrEmail);
    }
}
