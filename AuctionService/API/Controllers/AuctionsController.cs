using Asp.Versioning;
using AuctionService.Application.Commands.CreateAuction;
using AuctionService.Application.Commands.DeleteAuction;
using AuctionService.Application.Commands.UpdateAuction;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetAuctionById;
using AuctionService.Application.Queries.GetAuctions;
using Common.Core.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers
{
    /// <summary>
    /// Controller for auction operations - demonstrates CQRS pattern with MediatR
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuctionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get paginated list of auctions with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AuctionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<AuctionDto>>> GetAuctions(
            [FromQuery] string? status = null,
            [FromQuery] string? seller = null,
            [FromQuery] string? winner = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool descending = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAuctionsQuery(
                status, seller, winner, searchTerm, 
                pageNumber, pageSize, orderBy, descending);

            var result = await _mediator.Send(query, cancellationToken);

            return result.IsSuccess 
                ? Ok(result.Value) 
                : BadRequest(CreateProblemDetails(result.Error!));
        }

        /// <summary>
        /// Get a single auction by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(
            Guid id, 
            CancellationToken cancellationToken)
        {
            var query = new GetAuctionByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            return result.IsSuccess 
                ? Ok(result.Value) 
                : NotFound(CreateProblemDetails(result.Error!));
        }

        /// <summary>
        /// Create a new auction
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AuctionWrite")]
        [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuctionDto>> CreateAuction(
            [FromBody] CreateAuctionDto createAuctionDto,
            CancellationToken cancellationToken)
        {
            var seller = User.Identity?.Name ?? "anonymous";

            var command = new CreateAuctionCommand(
                createAuctionDto.Title,
                createAuctionDto.Description,
                createAuctionDto.Make,
                createAuctionDto.Model,
                createAuctionDto.Year,
                createAuctionDto.Color,
                createAuctionDto.Mileage,
                createAuctionDto.ImageUrl,
                createAuctionDto.ReservePrice,
                createAuctionDto.AuctionEnd,
                seller);

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetAuctionById), new { id = result.Value!.Id }, result.Value)
                : BadRequest(CreateProblemDetails(result.Error!));
        }

        /// <summary>
        /// Update an existing auction
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AuctionOwner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateAuction(
            Guid id,
            [FromBody] UpdateAuctionDto updateAuctionDto,
            CancellationToken cancellationToken)
        {
            var command = new UpdateAuctionCommand(
                id,
                updateAuctionDto.Title,
                updateAuctionDto.Description,
                updateAuctionDto.Make,
                updateAuctionDto.Model,
                updateAuctionDto.Year,
                updateAuctionDto.Color,
                updateAuctionDto.Mileage);

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess 
                ? NoContent() 
                : NotFound(CreateProblemDetails(result.Error!));
        }

        /// <summary>
        /// Delete an auction
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AuctionOwner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAuction(
            Guid id, 
            CancellationToken cancellationToken)
        {
            var command = new DeleteAuctionCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess 
                ? NoContent() 
                : NotFound(CreateProblemDetails(result.Error!));
        }

        /// <summary>
        /// Creates RFC 7807 ProblemDetails from error
        /// </summary>
        private ProblemDetails CreateProblemDetails(Error error)
        {
            var problemDetails = new ProblemDetails
            {
                Title = error.Code,
                Detail = error.Message,
                Status = GetStatusCode(error),
                Type = $"https://api.auction.com/errors/{error.Code.ToLowerInvariant().Replace('.', '-')}"
            };

            // Include validation errors if present
            if (error is ValidationError validationError)
            {
                problemDetails.Extensions["errors"] = validationError.Errors;
            }

            return problemDetails;
        }

        private static int GetStatusCode(Error error)
        {
            return error.Code switch
            {
                var code when code.Contains("NotFound") => StatusCodes.Status404NotFound,
                var code when code.Contains("Validation") => StatusCodes.Status400BadRequest,
                var code when code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
                var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };
        }
    }
}
