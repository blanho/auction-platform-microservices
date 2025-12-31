using Common.Core.Authorization;
using Common.Core.Constants;
using IdentityService.DTOs;
using IdentityService.Interfaces;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IUserRepository userRepository,
        ILogger<AdminUsersController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<AdminUserListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminUserListDto>>> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isSuspended = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (users, totalCount) = await _userRepository.GetPagedAsync(
            search, role, isActive, isSuspended, pageNumber, pageSize, cancellationToken);

        var userDtos = new List<AdminUserDto>();

        foreach (var user in users)
        {
            var roles = await _userRepository.GetRolesAsync(user);

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
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetUser(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var roles = await _userRepository.GetRolesAsync(user);

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
    [HasPermission(Permissions.Users.Ban)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> SuspendUser(string id, [FromBody] SuspendUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsSuspended = true;
        user.SuspensionReason = dto.Reason;
        user.SuspendedAt = DateTimeOffset.UtcNow;

        var result = await _userRepository.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to suspend user"));
        }

        _logger.LogWarning("Admin suspended user {UserId} for reason: {Reason}", id, dto.Reason);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User suspended successfully"));
    }

    [HttpPost("{id}/unsuspend")]
    [HasPermission(Permissions.Users.Ban)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UnsuspendUser(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsSuspended = false;
        user.SuspensionReason = null;
        user.SuspendedAt = null;

        var result = await _userRepository.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to unsuspend user"));
        }

        _logger.LogInformation("Admin unsuspended user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User unsuspended successfully"));
    }

    [HttpPost("{id}/activate")]
    [HasPermission(Permissions.Users.Ban)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsActive = true;

        var result = await _userRepository.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to activate user"));
        }

        _logger.LogInformation("Admin activated user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User activated successfully"));
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(Permissions.Users.Ban)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        user.IsActive = false;

        var result = await _userRepository.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to deactivate user"));
        }

        _logger.LogInformation("Admin deactivated user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User deactivated successfully"));
    }

    [HttpPut("{id}/roles")]
    [HasPermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUserRoles(string id, [FromBody] UpdateUserRolesDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var currentRoles = await _userRepository.GetRolesAsync(user);
        await _userRepository.RemoveFromRolesAsync(user, currentRoles);

        foreach (var role in dto.Roles)
        {
            if (!await _userRepository.RoleExistsAsync(role))
            {
                await _userRepository.CreateRoleAsync(role);
            }
        }

        await _userRepository.AddToRolesAsync(user, dto.Roles);

        _logger.LogInformation("Admin updated roles for user {UserId}: {Roles}", id, string.Join(", ", dto.Roles));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User roles updated successfully"));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Users.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        var result = await _userRepository.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete user"));
        }

        _logger.LogWarning("Admin deleted user {UserId}", id);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "User deleted successfully"));
    }

    [HttpGet("stats")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<AdminStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminStatsDto>>> GetStats(CancellationToken cancellationToken = default)
    {
        var stats = await _userRepository.GetStatsAsync(cancellationToken);

        return Ok(ApiResponse<AdminStatsDto>.SuccessResponse(stats));
    }
}
