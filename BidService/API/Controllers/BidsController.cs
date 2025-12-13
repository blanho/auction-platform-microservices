using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidsController : ControllerBase
    {
        private readonly IBidService _bidService;
        private readonly ILogger<BidsController> _logger;

        public BidsController(IBidService bidService, ILogger<BidsController> logger)
        {
            _bidService = bidService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BidDto>> PlaceBid([FromBody] PlaceBidDto dto, CancellationToken cancellationToken)
        {
            var bidder = User.Identity?.Name ?? "Anonymous";
            var bid = await _bidService.PlaceBidAsync(dto, bidder, cancellationToken);
            return CreatedAtAction(nameof(GetBidsForAuction), new { auctionId = bid.AuctionId }, bid);
        }

        [HttpGet("auction/{auctionId:guid}")]
        public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(Guid auctionId, CancellationToken cancellationToken)
        {
            var bids = await _bidService.GetBidsForAuctionAsync(auctionId, cancellationToken);
            return Ok(bids);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<List<BidDto>>> GetMyBids(CancellationToken cancellationToken)
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized();
            }

            var bids = await _bidService.GetBidsForBidderAsync(currentUser, cancellationToken);
            return Ok(bids);
        }

        [HttpGet("bidder/{bidder}")]
        [Authorize]
        public async Task<ActionResult<List<BidDto>>> GetBidsForBidder(string bidder, CancellationToken cancellationToken)
        {
            var currentUser = User.Identity?.Name;
            if (currentUser != bidder && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var bids = await _bidService.GetBidsForBidderAsync(bidder, cancellationToken);
            return Ok(bids);
        }
    }
}
