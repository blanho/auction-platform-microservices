using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.Domain.Entities;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/v1/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly IPlatformSettingService _settingService;

    public SettingsController(IPlatformSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlatformSettingDto>>> GetSettings(
        [FromQuery] SettingCategory? category,
        CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetSettingsAsync(category, cancellationToken);
        return Ok(settings);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlatformSettingDto>> GetSetting(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var setting = await _settingService.GetSettingByIdAsync(id, cancellationToken);
            return Ok(setting);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("key/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<PlatformSettingDto>> GetSettingByKey(string key, CancellationToken cancellationToken)
    {
        try
        {
            var setting = await _settingService.GetSettingByKeyAsync(key, cancellationToken);
            return Ok(setting);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<PlatformSettingDto>> CreateSetting(
        [FromBody] CreateSettingDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var username = User.Identity?.Name;
            var setting = await _settingService.CreateSettingAsync(dto, username, cancellationToken);
            return CreatedAtAction(nameof(GetSetting), new { id = setting.Id }, setting);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSetting(
        Guid id,
        [FromBody] UpdateSettingDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var username = User.Identity?.Name;
            var setting = await _settingService.UpdateSettingAsync(id, dto, username, cancellationToken);
            return Ok(setting);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSetting(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _settingService.DeleteSettingAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdateSettings(
        [FromBody] BulkUpdateSettingsDto dto,
        CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        await _settingService.BulkUpdateSettingsAsync(dto.Settings, username, cancellationToken);
        return Ok();
    }
}

public class BulkUpdateSettingsDto
{
    public List<SettingKeyValue> Settings { get; set; } = new();
}
