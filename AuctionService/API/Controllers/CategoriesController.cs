using Asp.Versioning;
using AuctionService.Application.Commands.CreateCategory;
using AuctionService.Application.Commands.DeleteCategory;
using AuctionService.Application.Commands.UpdateCategory;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetCategories;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        IMediator mediator,
        ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories(
        [FromQuery] bool activeOnly = true,
        [FromQuery] bool includeCount = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(activeOnly, includeCount);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCategoryCommand(
            dto.Name,
            dto.Slug,
            dto.Icon,
            dto.Description,
            dto.ImageUrl,
            dto.DisplayOrder,
            dto.IsActive,
            dto.ParentCategoryId);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCategories), new { id = result.Value.Id }, result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCategoryCommand(
            id,
            dto.Name,
            dto.Slug,
            dto.Icon,
            dto.Description,
            dto.ImageUrl,
            dto.DisplayOrder,
            dto.IsActive,
            dto.ParentCategoryId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Category.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Category.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return NoContent();
    }
}
