using Common.Core.Authorization;
using IdentityService.DTOs;
using IdentityService.Interfaces;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/seller")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class SellerController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SellerController> _logger;

    public SellerController(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        ILogger<SellerController> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<SellerStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SellerStatusDto>>> GetSellerStatus()
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
        var isSeller = roles.Contains(AppRoles.Seller) || roles.Contains(AppRoles.Admin);

        return Ok(ApiResponse<SellerStatusDto>.SuccessResponse(new SellerStatusDto
        {
            IsSeller = isSeller,
            CanBecomeSeller = !isSeller && roles.Contains(AppRoles.User),
            Roles = roles.ToList()
        }));
    }

    [HttpPost("become-seller")]
    [ProducesResponseType(typeof(ApiResponse<SellerStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SellerStatusDto>>> BecomeSeller([FromBody] BecomeSellerDto dto)
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
        
        if (roles.Contains(AppRoles.Seller))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("You are already a seller"));
        }

        if (roles.Contains(AppRoles.Admin))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Admin accounts have seller privileges by default"));
        }

        if (!await _userRepository.RoleExistsAsync(AppRoles.Seller))
        {
            await _userRepository.CreateRoleAsync(AppRoles.Seller);
        }

        var result = await _userManager.AddToRoleAsync(user, AppRoles.Seller);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Failed to upgrade user {UserId} to seller: {Errors}", userId, string.Join(", ", errors));
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to upgrade to seller", errors));
        }

        _logger.LogInformation("User {UserId} upgraded to seller. Agreement accepted: {AgreementAccepted}", userId, dto.AcceptTerms);

        var updatedRoles = await _userManager.GetRolesAsync(user);

        return Ok(ApiResponse<SellerStatusDto>.SuccessResponse(
            new SellerStatusDto
            {
                IsSeller = true,
                CanBecomeSeller = false,
                Roles = updatedRoles.ToList()
            },
            "You are now a seller! You can start creating auctions."));
    }
}

public class SellerStatusDto
{
    public bool IsSeller { get; set; }
    public bool CanBecomeSeller { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class BecomeSellerDto
{
    public bool AcceptTerms { get; set; }
}
