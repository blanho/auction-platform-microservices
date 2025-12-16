#nullable enable
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Common.Core.Interfaces;
using Common.Core.Constants;
using Common.Domain.Enums;

namespace AuctionService.Infrastructure.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public AuctionRepository(AuctionDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<List<Auction>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Auction> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var auction = await _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            return auction ?? throw new KeyNotFoundException($"Auction with ID {id} not found");
        }

        public async Task<Auction> CreateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.CreatedAt = _dateTime.UtcNow;
            auction.CreatedBy = SystemGuids.System;
            auction.IsDeleted = false;
            
            if (auction.Item != null)
            {
                auction.Item.CreatedAt = _dateTime.UtcNow;
                auction.Item.CreatedBy = SystemGuids.System;
                auction.Item.IsDeleted = false;
            }
            
            await _context.Auctions.AddAsync(auction, cancellationToken);
            
            return auction;
        }

        public async Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.UpdatedAt = _dateTime.UtcNow;
            auction.UpdatedBy = SystemGuids.System;
            
            _context.Auctions.Update(auction);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var auction = await GetByIdAsync(id, cancellationToken);
            if (auction != null)
            {
                auction.IsDeleted = true;
                auction.DeletedAt = _dateTime.UtcNow;
                auction.DeletedBy = SystemGuids.System;
                
                _context.Auctions.Update(auction);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            
            return await _context.Auctions
                .Where(x => !x.IsDeleted 
                    && x.AuctionEnd < now 
                    && x.Status != Status.Finished 
                    && x.Status != Status.ReservedNotMet
                    && x.Status != Status.Inactive)
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            
            return await _context.Auctions
                .Where(x => !x.IsDeleted 
                    && x.AuctionEnd < now 
                    && x.Status == Status.Live)
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetAuctionsForExportAsync(
            Status? status = null,
            string? seller = null,
            DateTimeOffset? startDate = null,
            DateTimeOffset? endDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(seller))
            {
                query = query.Where(x => x.SellerUsername == seller);
            }

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetScheduledAuctionsToActivateAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted 
                    && x.Status == Status.Scheduled)
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetAuctionsEndingBetweenAsync(
            DateTime startTime, 
            DateTime endTime, 
            CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted 
                    && x.Status == Status.Live
                    && x.AuctionEnd >= startTime 
                    && x.AuctionEnd <= endTime)
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountLiveAuctionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.Status == Status.Live)
                .CountAsync(cancellationToken);
        }

        public async Task<int> CountEndingSoonAsync(CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            var endingSoonThreshold = now.AddHours(24);
            
            return await _context.Auctions
                .Where(x => !x.IsDeleted 
                    && x.Status == Status.Live 
                    && x.AuctionEnd >= now 
                    && x.AuctionEnd <= endingSoonThreshold)
                .CountAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.Status == Status.Live)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .OrderByDescending(x => x.CurrentHighBid ?? 0)
                .ThenByDescending(x => x.IsFeatured)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }
    }
}
