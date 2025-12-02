using Asp.Versioning;
using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers
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

        [HttpPost]
        [Authorize(Policy = "AuctionWrite")]
        public async Task<ActionResult<AuctionDto>> CreateAuction(
            CreateAuctionDto createAuctionDto,
            CancellationToken cancellationToken)
        {
            var seller = User.Identity?.Name ?? "test";
            var auction = await _auctionService.CreateAuctionAsync(createAuctionDto, seller, cancellationToken);

            return CreatedAtRoute(
                new { id = auction.Id },
                auction);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AuctionOwner")]
        public async Task<ActionResult> UpdateAuction(
            Guid id,
            UpdateAuctionDto updateAuctionDto,
            CancellationToken cancellationToken)
        {
            
            await _auctionService.UpdateAuctionAsync(id, updateAuctionDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AuctionOwner")]
        public async Task<ActionResult> DeleteAuction(Guid id, CancellationToken cancellationToken)
        {
            
            await _auctionService.DeleteAuctionAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
