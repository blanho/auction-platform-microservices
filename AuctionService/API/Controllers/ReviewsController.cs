#nullable enable
using Asp.Versioning;
using AuctionService.Application.Commands.AddSellerResponse;
using AuctionService.Application.Commands.CreateReview;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetReviewById;
using AuctionService.Application.Queries.GetReviewsByUser;
using AuctionService.Application.Queries.GetReviewsForAuction;
using AuctionService.Application.Queries.GetReviewsForUser;
using AuctionService.Application.Queries.GetUserRatingSummary;
using Common.Core.Helpers;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> GetReviewById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetReviewByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("auction/{auctionId:guid}")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewDto>>> GetReviewsForAuction(
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetReviewsForAuctionQuery(auctionId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("user/{username}")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewDto>>> GetReviewsForUser(
        string username,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetReviewsForUserQuery(username), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("by/{username}")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewDto>>> GetReviewsByUser(
        string username,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetReviewsByUserQuery(username), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("user/{username}/summary")]
    [ProducesResponseType(typeof(UserRatingSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserRatingSummaryDto>> GetUserRatingSummary(
        string username,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUserRatingSummaryQuery(username), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost]
    [Authorize(Policy = "AuctionScope")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewDto>> CreateReview(
        [FromBody] CreateReviewDto dto,
        CancellationToken cancellationToken = default)
    {
        var username = UserHelper.GetUsername(User);

        var command = new CreateReviewCommand(
            dto.AuctionId,
            dto.OrderId,
            username,
            dto.ReviewedUsername,
            dto.Rating,
            dto.Title,
            dto.Comment);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetReviewById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("{id:guid}/response")]
    [Authorize(Policy = "AuctionScope")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewDto>> AddSellerResponse(
        Guid id,
        [FromBody] AddSellerResponseDto dto,
        CancellationToken cancellationToken = default)
    {
        var username = UserHelper.GetUsername(User);

        var command = new AddSellerResponseCommand(id, username, dto.Response);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}
