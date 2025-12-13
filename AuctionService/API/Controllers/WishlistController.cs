#nullable enable
using Asp.Versioning;
using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auctions/wishlist")]
[Authorize(Policy = "AuctionScope")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IAuctionRepository _auctionRepository;

    public WishlistController(
        IWishlistRepository wishlistRepository,
        IAuctionRepository auctionRepository)
    {
        _wishlistRepository = wishlistRepository;
        _auctionRepository = auctionRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<WishlistItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WishlistItemDto>>> GetWishlist(CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var wishlistItems = await _wishlistRepository.GetByUsernameAsync(username, cancellationToken);
        
        var result = new List<WishlistItemDto>();
        foreach (var item in wishlistItems)
        {
            var auction = await _auctionRepository.GetByIdAsync(item.AuctionId, cancellationToken);
            if (auction != null)
            {
                result.Add(new WishlistItemDto
                {
                    Id = item.Id,
                    AuctionId = item.AuctionId,
                    AddedAt = item.AddedAt,
                    AuctionTitle = auction.Item.Title,
                    AuctionImageUrl = auction.Item.Files.FirstOrDefault(f => f.IsPrimary)?.Url 
                                     ?? auction.Item.Files.FirstOrDefault()?.Url,
                    CurrentBid = auction.CurrentHighBid ?? 0,
                    AuctionEnd = auction.AuctionEnd,
                    Status = auction.Status.ToString()
                });
            }
        }
        
        return Ok(result);
    }

    [HttpGet("ids")]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Guid>>> GetWishlistAuctionIds(CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var auctionIds = await _wishlistRepository.GetAuctionIdsByUsernameAsync(username, cancellationToken);
        return Ok(auctionIds);
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetWishlistCount(CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var count = await _wishlistRepository.GetCountByUsernameAsync(username, cancellationToken);
        return Ok(count);
    }

    [HttpGet("{auctionId:guid}/check")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CheckWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var exists = await _wishlistRepository.ExistsAsync(username, auctionId, cancellationToken);
        return Ok(exists);
    }

    [HttpPost("{auctionId:guid}")]
    [ProducesResponseType(typeof(WishlistItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WishlistItemDto>> AddToWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);
        if (auction == null)
        {
            return NotFound(new { message = "Auction not found" });
        }

        var existing = await _wishlistRepository.GetByUsernameAndAuctionAsync(username, auctionId, cancellationToken);
        if (existing != null)
        {
            return BadRequest(new { message = "Auction already in wishlist" });
        }

        var wishlistItem = new WishlistItem
        {
            Username = username,
            AuctionId = auctionId
        };

        var created = await _wishlistRepository.AddAsync(wishlistItem, cancellationToken);

        var result = new WishlistItemDto
        {
            Id = created.Id,
            AuctionId = created.AuctionId,
            AddedAt = created.AddedAt,
            AuctionTitle = auction.Item.Title,
            AuctionImageUrl = auction.Item.Files.FirstOrDefault(f => f.IsPrimary)?.Url 
                             ?? auction.Item.Files.FirstOrDefault()?.Url,
            CurrentBid = auction.CurrentHighBid ?? 0,
            AuctionEnd = auction.AuctionEnd,
            Status = auction.Status.ToString()
        };

        return CreatedAtAction(nameof(GetWishlist), result);
    }

    [HttpDelete("{auctionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        
        var existing = await _wishlistRepository.GetByUsernameAndAuctionAsync(username, auctionId, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Auction not in wishlist" });
        }

        await _wishlistRepository.RemoveAsync(existing.Id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{auctionId:guid}/toggle")]
    [ProducesResponseType(typeof(WishlistToggleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WishlistToggleResponse>> ToggleWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        
        var existing = await _wishlistRepository.GetByUsernameAndAuctionAsync(username, auctionId, cancellationToken);
        
        if (existing != null)
        {
            await _wishlistRepository.RemoveAsync(existing.Id, cancellationToken);
            return Ok(new WishlistToggleResponse { IsInWishlist = false, Message = "Removed from wishlist" });
        }
        
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);
        if (auction == null)
        {
            return NotFound(new { message = "Auction not found" });
        }

        var wishlistItem = new WishlistItem
        {
            Username = username,
            AuctionId = auctionId
        };

        await _wishlistRepository.AddAsync(wishlistItem, cancellationToken);
        return Ok(new WishlistToggleResponse { IsInWishlist = true, Message = "Added to wishlist" });
    }
}

public class WishlistToggleResponse
{
    public bool IsInWishlist { get; set; }
    public string Message { get; set; } = string.Empty;
}
