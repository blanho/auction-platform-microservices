using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AutoBidsController : ControllerBase
{
    private readonly IAutoBidService _autoBidService;

    public AutoBidsController(IAutoBidService autoBidService)
    {
        _autoBidService = autoBidService;
    }

    private (Guid UserId, string Username)? GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(username))
            return null;
            
        return (userId, username);
    }

    [HttpPost]
    public async Task<ActionResult<AutoBidDto>> CreateAutoBid([FromBody] CreateAutoBidDto dto)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var result = await _autoBidService.CreateAutoBidAsync(dto, user.Value.UserId, user.Value.Username);
        if (result == null)
            return BadRequest("Failed to create auto-bid");

        return CreatedAtAction(nameof(GetAutoBid), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AutoBidDto>> GetAutoBid(Guid id)
    {
        var autoBid = await _autoBidService.GetAutoBidByIdAsync(id);
        if (autoBid == null)
            return NotFound();

        return Ok(autoBid);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<AutoBidDto>>> GetMyAutoBids()
    {
        var user = GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var autoBids = await _autoBidService.GetAutoBidsByUserAsync(user.Value.UserId);
        return Ok(autoBids);
    }

    [HttpGet("auction/{auctionId}")]
    public async Task<ActionResult<AutoBidDto>> GetMyAutoBidForAuction(Guid auctionId)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var autoBid = await _autoBidService.GetActiveAutoBidAsync(auctionId, user.Value.UserId);
        if (autoBid == null)
            return NotFound();

        return Ok(autoBid);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AutoBidDto>> UpdateAutoBid(Guid id, [FromBody] UpdateAutoBidDto dto)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var result = await _autoBidService.UpdateAutoBidAsync(id, dto, user.Value.UserId);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAutoBid(Guid id)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var success = await _autoBidService.CancelAutoBidAsync(id, user.Value.UserId);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
