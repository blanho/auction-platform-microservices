using Asp.Versioning;
using AuctionService.Application.Commands.BulkUpdateCategories;
using AuctionService.Application.Commands.CreateCategory;
using AuctionService.Application.Commands.DeleteCategory;
using AuctionService.Application.Commands.ImportCategories;
using AuctionService.Application.Commands.UpdateCategory;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetCategories;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

    [HttpPost("bulk-update")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> BulkUpdateCategories(
        [FromBody] BulkUpdateCategoriesDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new BulkUpdateCategoriesCommand(dto.CategoryIds, dto.IsActive);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ImportCategoriesResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ImportCategoriesResultDto>> ImportCategories(
        [FromBody] ImportCategoriesDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new ImportCategoriesCommand(dto.Categories);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        return Ok(new ImportCategoriesResultDto
        {
            SuccessCount = result.Value.SuccessCount,
            FailureCount = result.Value.FailureCount,
            Errors = result.Value.Errors
        });
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportCategories(
        [FromQuery] string format = "json",
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(activeOnly ?? false, true);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        var categories = result.Value;

        return format.ToLower() switch
        {
            "csv" => ExportAsCsv(categories),
            _ => ExportAsJson(categories)
        };
    }

    private FileContentResult ExportAsJson(List<CategoryDto> categories)
    {
        var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"categories_export_{DateTime.UtcNow:yyyyMMdd}.json");
    }

    private FileContentResult ExportAsCsv(List<CategoryDto> categories)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Id,Name,Slug,Icon,Description,ImageUrl,DisplayOrder,IsActive,ParentCategoryId,AuctionCount");

        foreach (var cat in categories)
        {
            csv.AppendLine($"\"{cat.Id}\",\"{EscapeCsv(cat.Name)}\",\"{EscapeCsv(cat.Slug)}\",\"{EscapeCsv(cat.Icon)}\",\"{EscapeCsv(cat.Description ?? "")}\",\"{EscapeCsv(cat.ImageUrl ?? "")}\",{cat.DisplayOrder},{cat.IsActive},\"{cat.ParentCategoryId}\",{cat.AuctionCount}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"categories_export_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string EscapeCsv(string value)
    {
        return value.Replace("\"", "\"\"");
    }
}
