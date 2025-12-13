using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
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

public class ConfirmEmailDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ResendConfirmationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordDto
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

public class ExternalLoginDto
{
    public string Provider { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = "/";
}

public class ExternalLoginCallbackDto
{
    public string? RemoteError { get; set; }
    public string ReturnUrl { get; set; } = "/";
}

public class Enable2FADto
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

public class Verify2FADto
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
    
    public bool RememberDevice { get; set; }
}

public class TwoFactorSetupDto
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
}

public class TwoFactorStatusDto
{
    public bool IsEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool IsMachineRemembered { get; set; }
}

public class RecoveryCodesDto
{
    public List<string> RecoveryCodes { get; set; } = new();
}

public class Disable2FADto
{
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UseRecoveryCodeDto
{
    [Required]
    public string RecoveryCode { get; set; } = string.Empty;
}
