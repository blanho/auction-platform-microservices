using System.ComponentModel.DataAnnotations;

namespace Identity.Api.DTOs.TwoFactor;

public class Enable2FARequest
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

public class Verify2FARequest
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
    
    public bool RememberDevice { get; set; }
}

public class Disable2FARequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UseRecoveryCodeRequest
{
    [Required]
    public string RecoveryCode { get; set; } = string.Empty;
}

public class TwoFactorLoginRequest
{
    [Required]
    public string TwoFactorStateToken { get; set; } = string.Empty;

    [Required]
    [StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

public class TwoFactorSetupResponse
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
}

public class TwoFactorStatusResponse
{
    public bool IsEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool IsMachineRemembered { get; set; }
}

public class RecoveryCodesResponse
{
    public List<string> RecoveryCodes { get; set; } = new();
}
