using Asp.Versioning;
using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using BidService.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BidService.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/bids")]
    [EnableRateLimiting("api")]
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
        [EnableRateLimiting("bid")]
        public async Task<ActionResult<BidDto>> PlaceBid([FromBody] PlaceBidDto dto, CancellationToken cancellationToken)
        {
            var bidder = User.Identity?.Name ?? "Anonymous";
            var bid = await _bidService.PlaceBidAsync(dto, bidder, cancellationToken);
            
            if (!string.IsNullOrEmpty(bid.ErrorMessage))
            {
                return BadRequest(new { error = bid.ErrorMessage, bid });
            }
            
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

        [HttpGet("increment/{currentBid:int}")]
        public ActionResult<BidIncrementInfoDto> GetBidIncrementInfo(int currentBid)
        {
            var minimumIncrement = BidIncrement.GetMinimumIncrement(currentBid);
            var minimumNextBid = BidIncrement.GetMinimumNextBid(currentBid);

            return Ok(new BidIncrementInfoDto
            {
                CurrentBid = currentBid,
                MinimumIncrement = minimumIncrement,
                MinimumNextBid = minimumNextBid
            });
        }
    }

    public class BidIncrementInfoDto
    {
        public int CurrentBid { get; set; }
        public int MinimumIncrement { get; set; }
        public int MinimumNextBid { get; set; }
    }
}
