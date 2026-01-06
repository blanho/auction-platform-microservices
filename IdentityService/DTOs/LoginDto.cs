using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class LoginDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class LoginResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public bool RequiresTwoFactor { get; set; }
    
    /// <summary>
    /// Secure state token for 2FA verification. Only set when RequiresTwoFactor is true.
    /// This token proves the user already verified their password and must be sent
    /// to the login-2fa endpoint along with the 2FA code.
    /// </summary>
    public string? TwoFactorStateToken { get; set; }
}

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
