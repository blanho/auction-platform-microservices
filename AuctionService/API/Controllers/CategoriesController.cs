using Asp.Versioning;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.GetCategories;
using Common.Utilities.Helpers;
using MediatR;
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
}
