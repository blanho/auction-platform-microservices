using IdentityService.DTOs;
using IdentityService.Interfaces;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Web;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenGenerationService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IDistributedCache _cache;
    private const string ClientId = "nextApp";
    private const string RefreshTokenCookieName = "refreshToken";
    private const int RefreshTokenCookieExpirationDays = 7;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenGenerationService tokenService,
        IEmailService emailService,
        IAuthService authService,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        IDistributedCache cache)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    [HttpPost("register")]
    [EnableRateLimiting("registration")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", GetModelStateErrors()));

        var result = await _authService.RegisterAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message!, result.Errors));

        return CreatedAtAction(
            nameof(GetCurrentUser),
            ApiResponse<UserDto>.SuccessResponse(result.Data!, result.Message)
        );
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        var result = await _authService.ConfirmEmailAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message!, result.Errors));

        return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
    }

    [HttpPost("resend-confirmation")]
    [EnableRateLimiting("password-reset")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResendConfirmation([FromBody] ResendConfirmationDto dto)
    {
        var result = await _authService.ResendConfirmationAsync(dto.Email);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message!, result.Errors));

        return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("password-reset")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", GetModelStateErrors()));

        var result = await _authService.ResetPasswordAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message!, result.Errors));

        return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", GetModelStateErrors()));

        var ipAddress = GetIpAddress();
        var result = await _authService.LoginAsync(dto, ipAddress!);

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<object>.ErrorResponse(result.Message!, result.Errors));

        if (result.RequiresTwoFactor)
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result.Data!, result.Message));

        if (!string.IsNullOrEmpty(result.Data?.RefreshToken))
            SetRefreshTokenCookie(result.Data.RefreshToken);

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result.Data!, result.Message));
    }

    [HttpPost("login-2fa")]
    [EnableRateLimiting("2fa")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginWith2FA([FromBody] TwoFactorLoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", GetModelStateErrors()));

        var ipAddress = GetIpAddress();
        var result = await _authService.LoginWith2FAAsync(dto, ipAddress!);

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<object>.ErrorResponse(result.Message!, result.Errors));

        if (!string.IsNullOrEmpty(result.Data?.RefreshToken))
            SetRefreshTokenCookie(result.Data.RefreshToken);

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result.Data!, result.Message));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponseDto>>> RefreshToken()
    {
        var refreshToken = GetRefreshTokenFromCookie();

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Refresh token not found"));
        }

        var ipAddress = GetIpAddress();
        var tokens = await _tokenService.RefreshTokenAsync(refreshToken, ClientId, ipAddress);

        if (tokens == null)
        {
            ClearRefreshTokenCookie();
            _logger.LogWarning("Invalid refresh token attempt from {IpAddress}", ipAddress);
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid or expired refresh token"));
        }

        _logger.LogInformation("Token refreshed successfully from {IpAddress}", ipAddress);

        SetRefreshTokenCookie(tokens.Value.RefreshToken);

        return Ok(ApiResponse<RefreshTokenResponseDto>.SuccessResponse(new RefreshTokenResponseDto
        {
            AccessToken = tokens.Value.AccessToken,
            ExpiresIn = tokens.Value.ExpiresIn
        }, "Token refreshed successfully"));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        var ipAddress = GetIpAddress();
        var refreshToken = GetRefreshTokenFromCookie();

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        }
        else
        {
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _tokenService.RevokeAllUserTokensAsync(userId, ipAddress);
            }
        }

        ClearRefreshTokenCookie();

        _logger.LogInformation("User logged out successfully");

        return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out successfully"));
    }

    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> LogoutAll()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var ipAddress = GetIpAddress();
        await _tokenService.RevokeAllUserTokensAsync(userId, ipAddress);

        ClearRefreshTokenCookie();

        _logger.LogInformation("All sessions revoked for user {UserId}", userId);

        return Ok(ApiResponse<object>.SuccessResponse(null, "All sessions have been logged out"));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? Common.Core.Authorization.AppRoles.User;

        return Ok(ApiResponse<UserDto>.SuccessResponse(new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = primaryRole,
            Roles = roles.ToList(),
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        }));
    }

    [HttpGet("external-providers")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetExternalProviders()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var providers = schemes.Select(s => s.Name).ToList();
        return Ok(ApiResponse<List<string>>.SuccessResponse(providers));
    }

    [HttpPost("external-user")]
    [ProducesResponseType(typeof(ExternalUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrGetExternalUser([FromBody] CreateExternalUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Provider) || string.IsNullOrEmpty(request.ProviderKey))
        {
            return BadRequest(new { error = "Email, provider, and providerKey are required" });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            var username = GenerateUniqueUsername(request.Name ?? request.Email.Split('@')[0]);
            user = new ApplicationUser
            {
                UserName = username,
                Email = request.Email,
                EmailConfirmed = true,
                FullName = request.Name
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user from external login: {Errors}", errors);
                return BadRequest(new { error = "Failed to create account", details = errors });
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(request.Provider, request.ProviderKey, request.Provider));
            if (!addLoginResult.Succeeded)
            {
                _logger.LogWarning("Failed to add external login for user {UserId}", user.Id);
            }

            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            }

            _logger.LogInformation("Created new user {Username} via {Provider}", user.UserName, request.Provider);
        }
        else
        {
            if (user.IsSuspended)
            {
                return BadRequest(new { error = "Account is suspended" });
            }

            var existingLogins = await _userManager.GetLoginsAsync(user);
            if (!existingLogins.Any(l => l.LoginProvider == request.Provider && l.ProviderKey == request.ProviderKey))
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(request.Provider, request.ProviderKey, request.Provider));
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add external login for user {UserId}", user.Id);
                }
            }
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "user";

        var tokens = await _tokenService.GenerateTokensForUserAsync(user.Id, ClientId);
        if (tokens == null)
        {
            _logger.LogError("Failed to generate tokens for external user {UserId}", user.Id);
            return BadRequest(new { error = "Failed to generate authentication tokens" });
        }

        _logger.LogInformation("User {Username} authenticated via {Provider}", user.UserName, request.Provider);

        return Ok(new ExternalUserResponse
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = role,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresIn = tokens.ExpiresIn
        });
    }

    [HttpGet("external-login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        returnUrl ??= frontendUrl;
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        returnUrl ??= frontendUrl;

        if (remoteError != null)
        {
            _logger.LogError("External login error: {Error}", remoteError);
            return Redirect($"{frontendUrl}/auth/signin?error={HttpUtility.UrlEncode(remoteError)}");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogError("External login info is null");
            return Redirect($"{frontendUrl}/auth/signin?error=External+login+failed");
        }

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Redirect($"{frontendUrl}/auth/signin?error=Email+not+provided+by+external+provider");
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            var username = GenerateUniqueUsername(name ?? email.Split('@')[0]);
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FullName = name
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user from external login: {Errors}", errors);
                return Redirect($"{frontendUrl}/auth/signin?error=Failed+to+create+account");
            }

            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            }
        }
        else if (user.IsSuspended)
        {
            return Redirect($"{frontendUrl}/auth/signin?error=Account+is+suspended");
        }

        var existingLogins = await _userManager.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
        {
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                _logger.LogWarning("Failed to add external login for user {UserId}", user.Id);
            }
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
        var authCode = await GenerateAuthorizationCodeAsync(user.Id, ClientId);

        _logger.LogInformation("User {Username} logged in with {Provider}", user.UserName, info.LoginProvider);
        return Redirect($"{returnUrl}?code={authCode}");
    }

    /// <summary>
    /// Exchange authorization code for tokens. This is more secure than passing tokens in URL.
    /// </summary>
    [HttpPost("exchange-code")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ExchangeCodeForTokens([FromBody] ExchangeCodeRequest request)
    {
        if (string.IsNullOrEmpty(request.Code))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Authorization code is required"));
        }

        var cacheKey = $"auth-code:{request.Code}";
        var cached = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(cached))
        {
            _logger.LogWarning("Invalid or expired authorization code attempt");
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid or expired authorization code"));
        }
        await _cache.RemoveAsync(cacheKey);

        var codeData = JsonSerializer.Deserialize<AuthCodeData>(cached);
        if (codeData == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid authorization code data"));
        }

        var user = await _userManager.FindByIdAsync(codeData.UserId);
        if (user == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var ipAddress = GetIpAddress();
        var tokens = await _tokenService.GenerateTokenPairAsync(user.Id, ClientId, ipAddress);
        if (tokens == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to generate tokens"));
        }

        SetRefreshTokenCookie(tokens.Value.RefreshToken);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "user";

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = role,
            AccessToken = tokens.Value.AccessToken,
            ExpiresIn = tokens.Value.ExpiresIn,
            RequiresTwoFactor = false
        }, "Login successful"));
    }

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

    private async Task<ApplicationUser?> FindUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (usernameOrEmail.Contains('@'))
        {
            return await _userManager.FindByEmailAsync(usernameOrEmail);
        }
        return await _userManager.FindByNameAsync(usernameOrEmail);
    }

    private List<string> GetModelStateErrors()
    {
        return ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
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

    private string? GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var isProduction = !string.Equals(
            _configuration["ASPNETCORE_ENVIRONMENT"],
            "Development",
            StringComparison.OrdinalIgnoreCase);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(RefreshTokenCookieExpirationDays),
            Path = "/api/auth"
        };

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        var isProduction = !string.Equals(
            _configuration["ASPNETCORE_ENVIRONMENT"],
            "Development",
            StringComparison.OrdinalIgnoreCase);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Path = "/api/auth"
        };

        Response.Cookies.Append(RefreshTokenCookieName, string.Empty, cookieOptions);
    }

    private string? GetRefreshTokenFromCookie()
    {
        return Request.Cookies[RefreshTokenCookieName];
    }

    private async Task<string> GenerateAuthorizationCodeAsync(string userId, string clientId)
    {
        var code = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
            
        var cacheKey = $"auth-code:{code}";
        var codeData = new AuthCodeData
        {
            UserId = userId,
            ClientId = clientId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(codeData), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) // Very short-lived
        });

        return code;
    }

    private class AuthCodeData
    {
        public string UserId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
