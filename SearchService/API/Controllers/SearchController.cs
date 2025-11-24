using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SearchService.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Search auctions with filters, sorting, and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<SearchResultDto>> Search([FromQuery] SearchRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _searchService.SearchAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get all search items (for admin/debug purposes)
        /// </summary>
        [HttpGet("items")]
        [AllowAnonymous]
        public async Task<ActionResult<List<SearchItemDto>>> GetAllItems(CancellationToken cancellationToken)
        {
            var items = await _searchService.GetAllItemsAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Get a specific search item by ID
        /// </summary>
        [HttpGet("items/{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<SearchItemDto>> GetItemById(Guid id, CancellationToken cancellationToken)
        {
            var item = await _searchService.GetItemByIdAsync(id, cancellationToken);
            return Ok(item);
        }

        /// <summary>
        /// Reindex all search items from source (admin operation)
        /// Note: Create/Update/Delete operations are handled via message queue consumers
        /// </summary>
        [HttpPost("reindex")]
        [Authorize]
        public async Task<ActionResult> ReindexAll(CancellationToken cancellationToken)
        {
            await _searchService.ReindexAllAsync(cancellationToken);
            return Ok(new { message = "Reindexing started successfully" });
        }
    }
}