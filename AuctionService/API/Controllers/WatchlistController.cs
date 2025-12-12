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
[Route("api/v{version:apiVersion}/watchlist")]
[Authorize(Policy = "AuctionScope")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistRepository _watchlistRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WatchlistController> _logger;

    public WatchlistController(
        IWatchlistRepository watchlistRepository,
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        ILogger<WatchlistController> logger)
    {
        _watchlistRepository = watchlistRepository;
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<WatchlistItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WatchlistItemDto>>> GetWatchlist(CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var items = await _watchlistRepository.GetByUsernameAsync(username, cancellationToken);

        var dtos = items.Select(item => new WatchlistItemDto
        {
            Id = item.Id,
            AuctionId = item.AuctionId,
            AuctionTitle = item.Auction?.Item?.Title ?? string.Empty,
            ImageUrl = item.Auction?.Item?.Files.FirstOrDefault(f => f.IsPrimary)?.Url,
            CurrentBid = item.Auction?.CurrentHighBid ?? 0,
            ReservePrice = item.Auction?.ReversePrice ?? 0,
            AuctionEnd = item.Auction?.AuctionEnd ?? DateTimeOffset.MinValue,
            Status = item.Auction?.Status.ToString() ?? string.Empty,
            AddedAt = item.AddedAt,
            NotifyOnBid = item.NotifyOnBid,
            NotifyOnEnd = item.NotifyOnEnd
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetWatchlistCount(CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var count = await _watchlistRepository.GetWatchlistCountAsync(username, cancellationToken);
        return Ok(count);
    }

    [HttpGet("check/{auctionId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsInWatchlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var isInWatchlist = await _watchlistRepository.IsInWatchlistAsync(username, auctionId, cancellationToken);
        return Ok(isInWatchlist);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WatchlistItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WatchlistItemDto>> AddToWatchlist(
        [FromBody] AddToWatchlistDto dto,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);

        var auction = await _auctionRepository.GetByIdAsync(dto.AuctionId, cancellationToken);
        if (auction == null)
        {
            return NotFound("Auction not found");
        }

        var existing = await _watchlistRepository.GetByUsernameAndAuctionAsync(username, dto.AuctionId, cancellationToken);
        if (existing != null)
        {
            return BadRequest("Auction is already in your watchlist");
        }

        var watchlistItem = new WatchlistItem
        {
            Username = username,
            AuctionId = dto.AuctionId,
            NotifyOnBid = dto.NotifyOnBid,
            NotifyOnEnd = dto.NotifyOnEnd,
            AddedAt = DateTimeOffset.UtcNow
        };

        await _watchlistRepository.CreateAsync(watchlistItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} added auction {AuctionId} to watchlist", username, dto.AuctionId);

        var result = new WatchlistItemDto
        {
            Id = watchlistItem.Id,
            AuctionId = watchlistItem.AuctionId,
            AuctionTitle = auction.Item?.Title ?? string.Empty,
            ImageUrl = auction.Item?.Files.FirstOrDefault(f => f.IsPrimary)?.Url,
            CurrentBid = auction.CurrentHighBid ?? 0,
            ReservePrice = auction.ReversePrice,
            AuctionEnd = auction.AuctionEnd,
            Status = auction.Status.ToString(),
            AddedAt = watchlistItem.AddedAt,
            NotifyOnBid = watchlistItem.NotifyOnBid,
            NotifyOnEnd = watchlistItem.NotifyOnEnd
        };

        return CreatedAtAction(nameof(GetWatchlist), result);
    }

    [HttpDelete("{auctionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFromWatchlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var item = await _watchlistRepository.GetByUsernameAndAuctionAsync(username, auctionId, cancellationToken);

        if (item == null)
        {
            return NotFound("Item not found in watchlist");
        }

        await _watchlistRepository.DeleteAsync(item.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} removed auction {AuctionId} from watchlist", username, auctionId);

        return NoContent();
    }

    [HttpPut("{auctionId:guid}/notifications")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateNotificationSettings(
        Guid auctionId,
        [FromBody] UpdateWatchlistNotificationsDto dto,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var item = await _watchlistRepository.GetByUsernameAndAuctionAsync(username, auctionId, cancellationToken);

        if (item == null)
        {
            return NotFound("Item not found in watchlist");
        }

        item.NotifyOnBid = dto.NotifyOnBid;
        item.NotifyOnEnd = dto.NotifyOnEnd;

        await _watchlistRepository.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
