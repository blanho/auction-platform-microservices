#nullable enable
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class AuctionViewRepository : IAuctionViewRepository
{
    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public AuctionViewRepository(AuctionDbContext context, IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<int> GetViewCountForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.AuctionViews
            .Where(v => v.AuctionId == auctionId)
            .CountAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetViewCountsForAuctionsAsync(List<Guid> auctionIds, CancellationToken cancellationToken = default)
    {
        return await _context.AuctionViews
            .Where(v => auctionIds.Contains(v.AuctionId))
            .GroupBy(v => v.AuctionId)
            .Select(g => new { AuctionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AuctionId, x => x.Count, cancellationToken);
    }

    public async Task RecordViewAsync(Guid auctionId, string? userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var view = new AuctionView
        {
            AuctionId = auctionId,
            UserId = userId,
            IpAddress = ipAddress,
            ViewedAt = _dateTime.UtcNow
        };
        view.SetCreatedAudit(Guid.Empty, _dateTime.UtcNow);

        await _context.AuctionViews.AddAsync(view, cancellationToken);
    }
}
