using Asp.Versioning;
using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using BidService.Domain.ValueObjects;
using Common.Core.Authorization;
using Common.Idempotency.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BidService.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/bids")]
    [EnableRateLimiting("api")]
    public class BidsController : ControllerBase
    {
        private readonly IBidService _bidService;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<BidsController> _logger;

        public BidsController(
            IBidService bidService, 
            IIdempotencyService idempotencyService,
            ILogger<BidsController> logger)
        {
            _bidService = bidService;
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        [HttpPost]
        [HasPermission(Permissions.Bids.Place)]
        [EnableRateLimiting("bid")]
        public async Task<ActionResult<BidDto>> PlaceBid(
            [FromBody] PlaceBidDto dto, 
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            var username = User.Identity?.Name ?? "Anonymous";
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var bidderId))
            {
                return Unauthorized(new { error = "User identity not found" });
            }
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var fullKey = $"bid:{bidderId}:{dto.AuctionId}:{idempotencyKey}";
                var idempotencyResult = await _idempotencyService.TryStartProcessingAsync(
                    fullKey, 
                    TimeSpan.FromSeconds(30), 
                    cancellationToken);

                if (!idempotencyResult.CanProcess)
                {
                    if (idempotencyResult.Status == IdempotencyStatus.Completed)
                    {
                        var cachedBid = await _idempotencyService.GetResultAsync<BidDto>(fullKey, cancellationToken);
                        if (cachedBid != null)
                        {
                            Response.Headers.Append("X-Idempotency-Replayed", "true");
                            return Ok(cachedBid);
                        }
                    }

                    if (idempotencyResult.Status == IdempotencyStatus.Processing)
                    {
                        return Conflict(new { error = "A bid with this idempotency key is currently being processed" });
                    }
                }

                try
                {
                    var bid = await _bidService.PlaceBidAsync(dto, bidderId, username, cancellationToken);
                    
                    if (string.IsNullOrEmpty(bid.ErrorMessage))
                    {
                        await _idempotencyService.MarkAsProcessedAsync(fullKey, bid, TimeSpan.FromHours(24), cancellationToken);
                        Response.Headers.Append("X-Idempotency-Key", idempotencyKey);
                        return CreatedAtAction(nameof(GetBidsForAuction), new { auctionId = bid.AuctionId }, bid);
                    }

                    await _idempotencyService.MarkAsFailedAsync(fullKey, bid.ErrorMessage, cancellationToken);
                    return BadRequest(new { error = bid.ErrorMessage, bid });
                }
                catch (Exception ex)
                {
                    await _idempotencyService.MarkAsFailedAsync(fullKey, ex.Message, cancellationToken);
                    throw;
                }
            }
            var result = await _bidService.PlaceBidAsync(dto, bidderId, username, cancellationToken);
            
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return BadRequest(new { error = result.ErrorMessage, bid = result });
            }
            
            return CreatedAtAction(nameof(GetBidsForAuction), new { auctionId = result.AuctionId }, result);
        }

        [HttpGet("auction/{auctionId:guid}")]
        public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(Guid auctionId, CancellationToken cancellationToken)
        {
            var bids = await _bidService.GetBidsForAuctionAsync(auctionId, cancellationToken);
            return Ok(bids);
        }

        [HttpGet("my")]
        [HasPermission(Permissions.Bids.ViewOwn)]
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
        [HasPermission(Permissions.Bids.View)]
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

        [HttpGet("increment/{currentBid:decimal}")]
        public ActionResult<BidIncrementInfoDto> GetBidIncrementInfo(decimal currentBid)
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
        public decimal CurrentBid { get; set; }
        public decimal MinimumIncrement { get; set; }
        public decimal MinimumNextBid { get; set; }
    }
}
