using BuildingBlocks.Application.Abstractions;
using Identity.Api.DTOs.Auth;
using Identity.Api.DTOs.External;
using Identity.Api.DTOs.TwoFactor;
using Identity.Api.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Interfaces;

public interface IAuthService
{
    Task<Result<UserDto>> RegisterAsync(RegisterRequest request);
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<Result> ResendConfirmationAsync(string email);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress);
    Task<Result<LoginResponse>> LoginWith2FAAsync(TwoFactorLoginRequest request, string ipAddress);
    Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress);
    Task<Result> LogoutAsync(string userId, string refreshToken);
    Task<Result> LogoutAllAsync(string userId);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
    Task<Result<ExternalAuthResult>> ProcessExternalLoginAsync(ExternalLoginInfo info);
    Task<Result<string>> GenerateAuthCodeAsync(string userId);
    Task<Result<LoginResponse>> ExchangeCodeForTokensAsync(string code, string ipAddress);

    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);

    Task<Result<UserDto>> GetCurrentUserAsync(string userId);
}
