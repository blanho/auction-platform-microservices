#nullable enable
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class AuctionBulkRepository : IAuctionBulkRepository
{
    private const int InsertBatchSize = 500;

    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAuditContext _auditContext;
    private readonly ILogger<AuctionBulkRepository> _logger;

    public AuctionBulkRepository(
        AuctionDbContext context,
        IDateTimeProvider dateTime,
        IAuditContext auditContext,
        ILogger<AuctionBulkRepository> logger)
    {
        _context = context;
        _dateTime = dateTime;
        _auditContext = auditContext;
        _logger = logger;
    }

    public async Task BulkInsertAsync(
        IReadOnlyList<Auction> auctions,
        CancellationToken cancellationToken = default)
    {
        if (auctions.Count == 0)
            return;

        var utcNow = _dateTime.UtcNowOffset;
        var totalInserted = 0;

        foreach (var batch in Chunk(auctions, InsertBatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var auction in batch)
            {
                auction.SetCreatedAudit(_auditContext.UserId, utcNow);
            }

            await _context.Auctions.AddRangeAsync(batch, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            totalInserted += batch.Count;

            _context.ChangeTracker.Clear();

            _logger.LogDebug(
                "Bulk inserted batch of {Count} auctions ({Total}/{Grand})",
                batch.Count, totalInserted, auctions.Count);
        }
    }

    public async Task<int> CountByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Auctions
            .Where(x => !x.IsDeleted)
            .CountAsync(cancellationToken);
    }

    private static List<List<T>> Chunk<T>(IReadOnlyList<T> source, int chunkSize)
    {
        var chunks = new List<List<T>>();
        for (var i = 0; i < source.Count; i += chunkSize)
        {
            var count = Math.Min(chunkSize, source.Count - i);
            chunks.Add(source.Skip(i).Take(count).ToList());
        }
        return chunks;
    }
}
