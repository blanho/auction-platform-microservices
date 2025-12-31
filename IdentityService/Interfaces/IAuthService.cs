using IdentityService.DTOs;

namespace IdentityService.Interfaces;

public interface IAuthService
{
    Task<AuthResult<UserDto>> RegisterAsync(RegisterDto dto);
    Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto dto);
    Task<AuthResult> ResendConfirmationAsync(string email);
    Task<AuthResult> ForgotPasswordAsync(string email);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto);
    Task<AuthResult<LoginResponseDto>> LoginAsync(LoginDto dto, string ipAddress);
    Task<AuthResult<LoginResponseDto>> LoginWith2FAAsync(TwoFactorLoginDto dto, string ipAddress);
    Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<AuthResult> RevokeTokenAsync(string refreshToken, string ipAddress);
    Task<AuthResult> LogoutAsync(string userId, string refreshToken);
    Task<AuthResult> ChangePasswordAsync(string userId, ChangePasswordDto dto);
}

public class AuthResult
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public List<string>? Errors { get; init; }

    public static AuthResult Success(string? message = null) => new() { IsSuccess = true, Message = message };
    public static AuthResult Failure(string message, List<string>? errors = null) => new() { IsSuccess = false, Message = message, Errors = errors };
}

public class AuthResult<T> : AuthResult
{
    public T? Data { get; init; }
    public bool RequiresTwoFactor { get; init; }

    public static AuthResult<T> Success(T data, string? message = null) => new() { IsSuccess = true, Data = data, Message = message };
    public static new AuthResult<T> Failure(string message, List<string>? errors = null) => new() { IsSuccess = false, Message = message, Errors = errors };
    public static AuthResult<T> TwoFactorRequired(T data, string? message = null) => new() { IsSuccess = true, Data = data, Message = message, RequiresTwoFactor = true };
}
