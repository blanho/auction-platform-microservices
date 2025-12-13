using IdentityService.DTOs;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
[Produces("application/json")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminUsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AdminUserListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminUserListDto>>> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isSuspended = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(search) ||
                u.Email!.ToLower().Contains(search) ||
                (u.FullName != null && u.FullName.ToLower().Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (isSuspended.HasValue)
        {
            query = query.Where(u => u.IsSuspended == isSuspended.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<AdminUserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role))
            {
                continue;
            }

            userDtos.Add(new AdminUserDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                IsActive = user.IsActive,
                IsSuspended = user.IsSuspended,
                SuspensionReason = user.SuspensionReason,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles
            });
        }

        var result = new AdminUserListDto
        {
            Users = userDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<AdminUserListDto>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new AdminUserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            IsActive = user.IsActive,
            IsSuspended = user.IsSuspended,
            SuspensionReason = user.SuspensionReason,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles
        };

        return Ok(ApiResponse<AdminUserDto>.SuccessResponse(dto));
    }

    [HttpPost("{id}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> SuspendUser(string id, [FromBody] SuspendUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsSuspended = true;
        user.SuspensionReason = dto.Reason;
        user.SuspendedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to suspend user"));
        }

        _logger.LogWarning("Admin suspended user {UserId} for reason: {Reason}", id, dto.Reason);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User suspended successfully"));
    }

    [HttpPost("{id}/unsuspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UnsuspendUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsSuspended = false;
        user.SuspensionReason = null;
        user.SuspendedAt = null;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to unsuspend user"));
        }

        _logger.LogInformation("Admin unsuspended user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User unsuspended successfully"));
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsActive = true;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to activate user"));
        }

        _logger.LogInformation("Admin activated user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User activated successfully"));
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to deactivate user"));
        }

        _logger.LogInformation("Admin deactivated user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User deactivated successfully"));
    }

    [HttpPut("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUserRoles(string id, [FromBody] UpdateUserRolesDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        foreach (var role in dto.Roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await _userManager.AddToRolesAsync(user, dto.Roles);

        _logger.LogInformation("Admin updated roles for user {UserId}: {Roles}", id, string.Join(", ", dto.Roles));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User roles updated successfully"));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete user"));
        }

        _logger.LogWarning("Admin deleted user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User deleted successfully"));
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AdminStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminStatsDto>>> GetStats()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
        var suspendedUsers = await _userManager.Users.CountAsync(u => u.IsSuspended);
        var newUsersThisMonth = await _userManager.Users
            .CountAsync(u => u.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30));

        var stats = new AdminStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            SuspendedUsers = suspendedUsers,
            NewUsersThisMonth = newUsersThisMonth
        };

        return Ok(ApiResponse<AdminStatsDto>.SuccessResponse(stats));
    }
}
