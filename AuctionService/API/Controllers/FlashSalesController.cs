#nullable enable
using Asp.Versioning;
using AuctionService.Application.Commands.AddFlashSaleItem;
using AuctionService.Application.Commands.CreateFlashSale;
using AuctionService.Application.Commands.DeleteFlashSale;
using AuctionService.Application.Commands.RemoveFlashSaleItem;
using AuctionService.Application.Commands.UpdateFlashSale;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetActiveFlashSale;
using AuctionService.Application.Queries.GetFlashSaleById;
using AuctionService.Application.Queries.GetFlashSales;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FlashSalesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FlashSalesController> _logger;

    public FlashSalesController(
        IMediator mediator,
        ILogger<FlashSalesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<FlashSaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FlashSaleDto>>> GetFlashSales(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFlashSalesQuery(activeOnly);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ActiveFlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ActiveFlashSaleDto?>> GetActiveFlashSale(
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveFlashSaleQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return NoContent();

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlashSaleDto>> GetFlashSaleById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFlashSaleByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "FlashSale.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FlashSaleDto>> CreateFlashSale(
        [FromBody] CreateFlashSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateFlashSaleCommand(
            dto.Title,
            dto.Description,
            dto.BannerUrl,
            dto.StartTime,
            dto.EndTime,
            dto.DiscountPercentage,
            dto.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetFlashSaleById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlashSaleDto>> UpdateFlashSale(
        Guid id,
        [FromBody] UpdateFlashSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateFlashSaleCommand(
            id,
            dto.Title,
            dto.Description,
            dto.BannerUrl,
            dto.StartTime,
            dto.EndTime,
            dto.DiscountPercentage,
            dto.IsActive,
            dto.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "FlashSale.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFlashSale(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteFlashSaleCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "FlashSale.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/items")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(FlashSaleItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlashSaleItemDto>> AddFlashSaleItem(
        Guid id,
        [FromBody] AddFlashSaleItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new AddFlashSaleItemCommand(
            id,
            dto.AuctionId,
            dto.SpecialPrice,
            dto.DiscountPercentage,
            dto.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "FlashSale.NotFound" || result.Error?.Code == "Auction.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return CreatedAtAction(nameof(GetFlashSaleById), new { id }, result.Value);
    }

    [HttpDelete("{id:guid}/items/{auctionId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFlashSaleItem(
        Guid id,
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveFlashSaleItemCommand(id, auctionId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "FlashSale.NotFound" || result.Error?.Code == "FlashSale.ItemNotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return NoContent();
    }
}
