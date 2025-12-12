using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace IdentityService.Controllers;
[ApiController]
[Route("api/account")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", errors));
        }

        var existingUser = await _userManager.FindByNameAsync(dto.Username);
        if (existingUser != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Username already exists"));
        }

        existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email already registered"));
        }

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            EmailConfirmed = false // Require email confirmation
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("User registration failed for {Username}: {Errors}", 
                dto.Username, string.Join(", ", errors));
            
            return BadRequest(ApiResponse<object>.ErrorResponse("Registration failed", errors));
        }

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var confirmationLink = $"{frontendUrl}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        try
        {
            await _emailService.SendEmailConfirmationAsync(user.Email!, user.UserName!, confirmationLink);
            _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
        }

        _logger.LogInformation("User {Username} registered successfully, awaiting email confirmation", dto.Username);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        };

        return CreatedAtAction(
            nameof(GetUser), 
            new { id = user.Id }, 
            ApiResponse<UserDto>.SuccessResponse(userDto, "Registration successful. Please check your email to confirm your account.")
        );
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid confirmation link"));
        }

        if (user.EmailConfirmed)
        {
            return Ok(ApiResponse<object>.SuccessResponse(null, "Email already confirmed"));
        }

        var decodedToken = HttpUtility.UrlDecode(dto.Token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Email confirmation failed for {UserId}: {Errors}", 
                dto.UserId, string.Join(", ", errors));
            return BadRequest(ApiResponse<object>.ErrorResponse("Email confirmation failed. The link may have expired.", errors));
        }

        // Send welcome email
        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        _logger.LogInformation("Email confirmed for user {Username}", user.UserName);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Email confirmed successfully. You can now log in."));
    }

    [HttpPost("resend-confirmation")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResendConfirmation([FromBody] ResendConfirmationDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            return Ok(ApiResponse<object>.SuccessResponse(null, "If an account exists with this email, a confirmation link will be sent."));
        }

        if (user.EmailConfirmed)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email is already confirmed"));
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var confirmationLink = $"{frontendUrl}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        try
        {
            await _emailService.SendEmailConfirmationAsync(user.Email!, user.UserName!, confirmationLink);
            _logger.LogInformation("Confirmation email resent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend confirmation email to {Email}", user.Email);
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to send email. Please try again later."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Confirmation email sent. Please check your inbox."));
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        // Always return success to prevent email enumeration
        if (user == null || !user.EmailConfirmed)
        {
            _logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", dto.Email);
            return Ok(ApiResponse<object>.SuccessResponse(null, "If an account exists with this email, a password reset link will be sent."));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/auth/reset-password?email={HttpUtility.UrlEncode(user.Email!)}&token={encodedToken}";

        try
        {
            await _emailService.SendPasswordResetAsync(user.Email!, user.UserName!, resetLink);
            _logger.LogInformation("Password reset email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "If an account exists with this email, a password reset link will be sent."));
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", errors));
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid reset request"));
        }

        var decodedToken = HttpUtility.UrlDecode(dto.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Password reset failed for {Email}: {Errors}", 
                dto.Email, string.Join(", ", errors));
            return BadRequest(ApiResponse<object>.ErrorResponse("Password reset failed. The link may have expired.", errors));
        }

        _logger.LogInformation("Password reset successful for {Email}", dto.Email);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successful. You can now log in with your new password."));
    }

    [HttpGet("external-providers")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetExternalProviders()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var providers = schemes.Select(s => s.Name).ToList();
        return Ok(ApiResponse<List<string>>.SuccessResponse(providers));
    }

    [HttpPost("external-login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult ExternalLogin([FromBody] ExternalLoginDto dto)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl = dto.ReturnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(dto.Provider, redirectUrl);
        return Challenge(properties, dto.Provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        returnUrl ??= $"{frontendUrl}/dashboard";

        if (remoteError != null)
        {
            _logger.LogError("External login error: {Error}", remoteError);
            return Redirect($"{frontendUrl}/auth/login?error={HttpUtility.UrlEncode(remoteError)}");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogError("External login info is null");
            return Redirect($"{frontendUrl}/auth/login?error=External+login+failed");
        }

        // Sign in the user with this external login provider if the user already has a login
        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            _logger.LogInformation("User logged in with {Provider}", info.LoginProvider);
            return Redirect(returnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            return Redirect($"{frontendUrl}/auth/login?error=Account+is+locked");
        }

        // If the user does not have an account, create one
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Redirect($"{frontendUrl}/auth/login?error=Email+not+provided+by+external+provider");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Create new user
            var username = GenerateUniqueUsername(name ?? email.Split('@')[0]);
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true, // External providers verify email
                FullName = name
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user from external login: {Errors}", errors);
                return Redirect($"{frontendUrl}/auth/login?error=Failed+to+create+account");
            }

            // Send welcome email
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            }
        }

        // Link the external login to the user
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            _logger.LogError("Failed to add external login for user {UserId}", user.Id);
        }

        // Sign in the user
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("User {Username} logged in with {Provider}", user.UserName, info.LoginProvider);

        return Redirect(returnUrl);
    }

    private string GenerateUniqueUsername(string baseName)
    {
        var sanitized = new string(baseName.Where(c => char.IsLetterOrDigit(c)).ToArray());
        if (sanitized.Length < 3)
        {
            sanitized = "user";
        }
        
        var username = sanitized;
        var counter = 1;
        
        while (_userManager.FindByNameAsync(username).Result != null)
        {
            username = $"{sanitized}{counter++}";
        }
        
        return username;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        };

        return Ok(ApiResponse<UserDto>.SuccessResponse(userDto));
    }

    /// <summary>
    /// Check if username is available
    /// </summary>
    [HttpGet("check-username/{username}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Username is required"));
        }

        var user = await _userManager.FindByNameAsync(username);
        var available = user == null;

        return Ok(ApiResponse<bool>.SuccessResponse(available));
    }

    /// <summary>
    /// Check if email is available
    /// </summary>
    [HttpGet("check-email/{email}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email is required"));
        }

        var user = await _userManager.FindByEmailAsync(email);
        var available = user == null;

        return Ok(ApiResponse<bool>.SuccessResponse(available));
    }
}
