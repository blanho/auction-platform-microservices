using IdentityService.DTOs;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirst("sub")?.Value 
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Bio = user.Bio,
            Location = user.Location,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles
        };

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.FullName = dto.FullName ?? user.FullName;
        user.Bio = dto.Bio ?? user.Bio;
        user.Location = dto.Location ?? user.Location;
        
        if (!string.IsNullOrEmpty(dto.PhoneNumber))
        {
            user.PhoneNumber = dto.PhoneNumber;
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update profile", errors));
        }

        _logger.LogInformation("User {UserId} updated their profile", userId);

        var roles = await _userManager.GetRolesAsync(user);
        var profile = new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Bio = user.Bio,
            Location = user.Location,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles
        };

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "Profile updated successfully"));
    }

    [HttpGet("notifications")]
    [ProducesResponseType(typeof(ApiResponse<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NotificationPreferencesDto>>> GetNotificationPreferences()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var preferences = new NotificationPreferencesDto
        {
            EmailBidUpdates = user.EmailBidUpdates,
            EmailOutbid = user.EmailOutbid,
            EmailAuctionEnd = user.EmailAuctionEnd,
            EmailNewsletter = user.EmailNewsletter,
            PushNotifications = user.PushNotifications,
            SmsNotifications = user.SmsNotifications
        };

        return Ok(ApiResponse<NotificationPreferencesDto>.SuccessResponse(preferences));
    }

    [HttpPut("notifications")]
    [ProducesResponseType(typeof(ApiResponse<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NotificationPreferencesDto>>> UpdateNotificationPreferences(
        [FromBody] NotificationPreferencesDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.EmailBidUpdates = dto.EmailBidUpdates;
        user.EmailOutbid = dto.EmailOutbid;
        user.EmailAuctionEnd = dto.EmailAuctionEnd;
        user.EmailNewsletter = dto.EmailNewsletter;
        user.PushNotifications = dto.PushNotifications;
        user.SmsNotifications = dto.SmsNotifications;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update preferences"));
        }

        _logger.LogInformation("User {UserId} updated notification preferences", userId);

        return Ok(ApiResponse<NotificationPreferencesDto>.SuccessResponse(dto, "Preferences updated successfully"));
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to change password", errors));
        }

        _logger.LogInformation("User {UserId} changed their password", userId);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Password changed successfully"));
    }

    [HttpPost("enable-2fa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> EnableTwoFactor()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        _logger.LogInformation("User {UserId} enabled 2FA", userId);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Two-factor authentication enabled"));
    }

    [HttpPost("disable-2fa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DisableTwoFactor()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);

        _logger.LogInformation("User {UserId} disabled 2FA", userId);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Two-factor authentication disabled"));
    }
}
