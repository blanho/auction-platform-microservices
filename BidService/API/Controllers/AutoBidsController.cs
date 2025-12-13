using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<ActionResult<AutoBidDto>> CreateAutoBid([FromBody] CreateAutoBidDto dto)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var result = await _autoBidService.CreateAutoBidAsync(dto, username);
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
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var autoBids = await _autoBidService.GetAutoBidsByBidderAsync(username);
        return Ok(autoBids);
    }

    [HttpGet("auction/{auctionId}")]
    public async Task<ActionResult<AutoBidDto>> GetMyAutoBidForAuction(Guid auctionId)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var autoBid = await _autoBidService.GetActiveAutoBidAsync(auctionId, username);
        if (autoBid == null)
            return NotFound();

        return Ok(autoBid);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AutoBidDto>> UpdateAutoBid(Guid id, [FromBody] UpdateAutoBidDto dto)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var result = await _autoBidService.UpdateAutoBidAsync(id, dto, username);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAutoBid(Guid id)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var success = await _autoBidService.CancelAutoBidAsync(id, username);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
