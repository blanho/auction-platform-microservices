using IdentityService.DTOs;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/account/2fa")]
[Authorize]
[Produces("application/json")]
public class TwoFactorController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UrlEncoder _urlEncoder;
    private readonly ILogger<TwoFactorController> _logger;
    
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    public TwoFactorController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UrlEncoder urlEncoder,
        ILogger<TwoFactorController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _urlEncoder = urlEncoder;
        _logger = logger;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<TwoFactorStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TwoFactorStatusDto>>> GetStatus()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(ApiResponse<TwoFactorStatusDto>.ErrorResponse("User not found"));
        }

        var status = new TwoFactorStatusDto
        {
            IsEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
        };

        return Ok(ApiResponse<TwoFactorStatusDto>.SuccessResponse(status));
    }

    [HttpPost("setup")]
    [ProducesResponseType(typeof(ApiResponse<TwoFactorSetupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TwoFactorSetupDto>>> SetupAuthenticator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(ApiResponse<TwoFactorSetupDto>.ErrorResponse("User not found"));
        }

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        
        if (string.IsNullOrEmpty(unformattedKey))
        {
            return BadRequest(ApiResponse<TwoFactorSetupDto>.ErrorResponse("Failed to generate authenticator key"));
        }

        var sharedKey = FormatKey(unformattedKey);
        var authenticatorUri = GenerateQrCodeUri(user.Email!, unformattedKey);

        var setup = new TwoFactorSetupDto
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeBase64 = string.Empty
        };

        _logger.LogInformation("2FA setup initiated for user {UserId}", user.Id);

        return Ok(ApiResponse<TwoFactorSetupDto>.SuccessResponse(setup, "Scan the QR code or enter the key in your authenticator app"));
    }

    [HttpPost("enable")]
    [ProducesResponseType(typeof(ApiResponse<RecoveryCodesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RecoveryCodesDto>>> EnableAuthenticator([FromBody] Enable2FADto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(ApiResponse<RecoveryCodesDto>.ErrorResponse("User not found"));
        }

        var verificationCode = dto.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, 
            _userManager.Options.Tokens.AuthenticatorTokenProvider, 
            verificationCode);

        if (!is2FaTokenValid)
        {
            _logger.LogWarning("Invalid 2FA verification code for user {UserId}", user.Id);
            return BadRequest(ApiResponse<RecoveryCodesDto>.ErrorResponse("Invalid verification code"));
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        
        _logger.LogInformation("2FA enabled for user {UserId}", user.Id);

        var response = new RecoveryCodesDto
        {
            RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>()
        };

        return Ok(ApiResponse<RecoveryCodesDto>.SuccessResponse(response, "Two-factor authentication has been enabled. Save your recovery codes in a safe place."));
    }

    [HttpPost("disable")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> DisableAuthenticator([FromBody] Disable2FADto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid password"));
        }

        var disable2FaResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2FaResult.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to disable 2FA"));
        }

        await _userManager.ResetAuthenticatorKeyAsync(user);
        
        _logger.LogInformation("2FA disabled for user {UserId}", user.Id);

        return Ok(ApiResponse<object>.SuccessResponse(null, "Two-factor authentication has been disabled"));
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> VerifyCode([FromBody] Verify2FADto dto)
    {
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            dto.Code.Replace(" ", string.Empty).Replace("-", string.Empty),
            isPersistent: false,
            rememberClient: dto.RememberDevice);

        if (result.Succeeded)
        {
            _logger.LogInformation("2FA verification successful");
            return Ok(ApiResponse<object>.SuccessResponse(null, "Verification successful"));
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out during 2FA verification");
            return BadRequest(ApiResponse<object>.ErrorResponse("Account locked out. Please try again later."));
        }

        _logger.LogWarning("Invalid 2FA code provided");
        return BadRequest(ApiResponse<object>.ErrorResponse("Invalid verification code"));
    }

    [HttpPost("recovery")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> UseRecoveryCode([FromBody] UseRecoveryCodeDto dto)
    {
        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(dto.RecoveryCode);

        if (result.Succeeded)
        {
            _logger.LogInformation("Recovery code used successfully");
            return Ok(ApiResponse<object>.SuccessResponse(null, "Recovery code accepted"));
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out during recovery code verification");
            return BadRequest(ApiResponse<object>.ErrorResponse("Account locked out. Please try again later."));
        }

        _logger.LogWarning("Invalid recovery code provided");
        return BadRequest(ApiResponse<object>.ErrorResponse("Invalid recovery code"));
    }

    [HttpPost("generate-codes")]
    [ProducesResponseType(typeof(ApiResponse<RecoveryCodesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RecoveryCodesDto>>> GenerateRecoveryCodes()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(ApiResponse<RecoveryCodesDto>.ErrorResponse("User not found"));
        }

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
        {
            return BadRequest(ApiResponse<RecoveryCodesDto>.ErrorResponse("Cannot generate recovery codes as 2FA is not enabled"));
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        
        _logger.LogInformation("New recovery codes generated for user {UserId}", user.Id);

        var response = new RecoveryCodesDto
        {
            RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>()
        };

        return Ok(ApiResponse<RecoveryCodesDto>.SuccessResponse(response, "New recovery codes have been generated"));
    }

    [HttpPost("forget-browser")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ForgetBrowser()
    {
        await _signInManager.ForgetTwoFactorClientAsync();
        return Ok(ApiResponse<object>.SuccessResponse(null, "Browser forgotten. You will need to verify 2FA on next login."));
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            AuthenticatorUriFormat,
            _urlEncoder.Encode("AuctionPlatform"),
            _urlEncoder.Encode(email),
            unformattedKey);
    }
}
