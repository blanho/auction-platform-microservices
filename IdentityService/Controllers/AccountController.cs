using IdentityService.DTOs;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;
[ApiController]
[Route("api/account")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
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
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("User registration failed for {Username}: {Errors}", 
                dto.Username, string.Join(", ", errors));
            
            return BadRequest(ApiResponse<object>.ErrorResponse("Registration failed", errors));
        }

        _logger.LogInformation("User {Username} registered successfully", dto.Username);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        };

        return CreatedAtAction(
            nameof(GetUser), 
            new { id = user.Id }, 
            ApiResponse<UserDto>.SuccessResponse(userDto, "Registration successful")
        );
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
