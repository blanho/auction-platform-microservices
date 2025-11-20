using Asp.Versioning;
using AuctionService.DTOs;
using AuctionService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionService _auctionService;

        public AuctionsController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(CancellationToken cancellationToken)
        {
            var auctions = await _auctionService.GetAllAuctionsAsync(cancellationToken);
            return Ok(auctions);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id, CancellationToken cancellationToken)
        {
            var auction = await _auctionService.GetAuctionByIdAsync(id, cancellationToken);

            if (auction == null)
            {
                return NotFound();
            }

            return Ok(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(
            CreateAuctionDto createAuctionDto, 
            CancellationToken cancellationToken)
        {
            var seller = User.Identity.Name ?? "test";
            var auction = await _auctionService.CreateAuctionAsync(createAuctionDto, seller, cancellationToken);

            if (auction == null)
            {
                return BadRequest("Could not save changes to the database");
            }

            return CreatedAtAction(
                nameof(GetAuctionById),
                new { id = auction.Id },
                auction);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateAuction(
            Guid id, 
            UpdateAuctionDto updateAuctionDto, 
            CancellationToken cancellationToken)
        {
            var result = await _auctionService.UpdateAuctionAsync(id, updateAuctionDto, cancellationToken);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteAuction(Guid id, CancellationToken cancellationToken)
        {
            var result = await _auctionService.DeleteAuctionAsync(id, cancellationToken);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
