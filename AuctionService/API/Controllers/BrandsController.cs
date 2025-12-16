#nullable enable
using Asp.Versioning;
using AuctionService.Application.Commands.CreateBrand;
using AuctionService.Application.Commands.DeleteBrand;
using AuctionService.Application.Commands.UpdateBrand;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetBrandById;
using AuctionService.Application.Queries.GetBrands;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(
        IMediator mediator,
        ILogger<BrandsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<BrandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BrandDto>>> GetBrands(
        [FromQuery] bool activeOnly = true,
        [FromQuery] bool featuredOnly = false,
        [FromQuery] int? count = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBrandsQuery(activeOnly, featuredOnly, count);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandDto>> GetBrandById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBrandByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Brand.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BrandDto>> CreateBrand(
        [FromBody] CreateBrandDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateBrandCommand(
            dto.Name,
            dto.LogoUrl,
            dto.Description,
            dto.DisplayOrder,
            dto.IsFeatured);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetBrandById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandDto>> UpdateBrand(
        Guid id,
        [FromBody] UpdateBrandDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateBrandCommand(
            id,
            dto.Name,
            dto.LogoUrl,
            dto.Description,
            dto.DisplayOrder,
            dto.IsActive,
            dto.IsFeatured);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Brand.NotFound")
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
    public async Task<IActionResult> DeleteBrand(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteBrandCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Brand.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return NoContent();
    }
}
