using System.ComponentModel.DataAnnotations;

namespace Identity.Api.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ConfirmEmailRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ResendConfirmationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string? TwoFactorStateToken { get; set; }
}

public record TokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn
);

public enum RefreshTokenFailureReason
{
    None = 0,
    TokenNotFound = 1,
    TokenExpired = 2,
    SecurityTermination = 3
}

public record RefreshTokenResult
{
    public bool IsSuccess { get; init; }
    public RefreshTokenFailureReason FailureReason { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public int ExpiresIn { get; init; }

    public static RefreshTokenResult Success(string accessToken, string refreshToken, int expiresIn) =>
        new()
        {
            IsSuccess = true,
            FailureReason = RefreshTokenFailureReason.None,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

    public static RefreshTokenResult Failure(RefreshTokenFailureReason reason) =>
        new()
        {
            IsSuccess = false,
            FailureReason = reason
        };
}
