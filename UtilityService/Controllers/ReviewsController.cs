using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto dto, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        try
        {
            var review = await _reviewService.CreateReviewAsync(dto, username, cancellationToken);
            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> GetReview(Guid id, CancellationToken cancellationToken)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, cancellationToken);
        if (review == null)
        {
            return NotFound();
        }

        return Ok(review);
    }

    [HttpGet("user/{username}")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviewsForUser(string username, CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsForUserAsync(username, cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("user/{username}/summary")]
    public async Task<ActionResult<UserRatingSummaryDto>> GetUserRatingSummary(string username, CancellationToken cancellationToken)
    {
        var summary = await _reviewService.GetUserRatingSummaryAsync(username, cancellationToken);
        return Ok(summary);
    }

    [HttpPost("{id:guid}/response")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> AddSellerResponse(Guid id, [FromBody] SellerResponseDto dto, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        try
        {
            var review = await _reviewService.AddSellerResponseAsync(id, dto, username, cancellationToken);
            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
