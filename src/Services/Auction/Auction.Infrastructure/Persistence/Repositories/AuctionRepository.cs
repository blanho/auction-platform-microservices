#nullable enable
using System.Linq.Expressions;
using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Paging;
using Auctions.Domain.Enums;

namespace Auctions.Infrastructure.Persistence.Repositories
{
    public class AuctionRepository : 
        IAuctionQueryRepository,
        IAuctionWriteRepository,
        IAuctionSchedulerRepository,
        IAuctionUserRepository,
        IAuctionExportRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTimeProvider _dateTime;
        private readonly IAuditContext _auditContext;

        private static readonly Dictionary<string, Expression<Func<Auction, object>>> AuctionSortMap =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["price"] = x => x.CurrentHighBid ?? x.ReservePrice,
            ["enddate"] = x => x.AuctionEnd,
            ["createdat"] = x => x.CreatedAt,
            ["title"] = x => x.Item != null ? x.Item.Title : string.Empty
        };

        public AuctionRepository(AuctionDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
        {
            _context = context;
            _dateTime = dateTime;
            _auditContext = auditContext;
        }

        public async Task<PaginatedResult<Auction>> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Auction>(items, totalCount, page, pageSize);
        }

        public async Task<PaginatedResult<Auction>> GetPagedAsync(
            AuctionFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var filterParams = filter.Filter;

            var query = _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterParams.Status) && Enum.TryParse<Status>(filterParams.Status, true, out var statusEnum))
            {
                query = query.Where(x => x.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(filterParams.Seller))
            {
                query = query.Where(x => x.SellerUsername == filterParams.Seller);
            }

            if (!string.IsNullOrEmpty(filterParams.Winner))
            {
                query = query.Where(x => x.WinnerUsername == filterParams.Winner);
            }

            if (!string.IsNullOrEmpty(filterParams.SearchTerm))
            {
                var term = $"%{filterParams.SearchTerm}%";
                query = query.Where(x =>
                    x.Item != null && (
                        EF.Functions.ILike(x.Item.Title, term) ||
                        (x.Item.Description != null && EF.Functions.ILike(x.Item.Description, term))
                    ));
            }

            if (!string.IsNullOrEmpty(filterParams.Category))
            {
                query = query.Where(x => x.Item != null && x.Item.Category != null && x.Item.Category.Name == filterParams.Category);
            }

            if (filterParams.IsFeatured.HasValue)
            {
                query = query.Where(x => x.IsFeatured == filterParams.IsFeatured.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = query.ApplySorting(filter, AuctionSortMap, x => x.UpdatedAt ?? x.CreatedAt);

            var items = await query
                .ApplyPaging(filter)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Auction>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Auction?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Auction> CreateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            auction.SetCreatedAudit(_auditContext.UserId, utcNow);

            if (auction.Item != null)
            {
                auction.Item.SetCreatedAudit(_auditContext.UserId, utcNow);
            }

            await _context.Auctions.AddAsync(auction, cancellationToken);

            return auction;
        }

        public Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.SetUpdatedAudit(_auditContext.UserId, _dateTime.UtcNow);

            _context.Auctions.Update(auction);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {

            var auction = await _context.Auctions
                .Where(x => !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (auction != null)
            {
                auction.MarkAsDeleted(_auditContext.UserId, _dateTime.UtcNow);
            }
        }

        public Task DeleteAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.MarkAsDeleted(_auditContext.UserId, _dateTime.UtcNow);
            return Task.CompletedTask;
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
                    && x.Status != Status.Inactive
                    && x.Status != Status.Cancelled
                    && x.Status != Status.ReservedForBuyNow)
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
                .AsNoTracking()
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

        public async Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.Status == Status.Live)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .OrderByDescending(x => x.CurrentHighBid ?? 0)
                .ThenByDescending(x => x.IsFeatured)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var idList = ids.ToList();
            if (idList.Count == 0)
                return new List<Auction>();

            return await _context.Auctions
                .Where(x => !x.IsDeleted && idList.Contains(x.Id))
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted &&
                           x.SellerId == sellerId &&
                           (x.Status == Status.Live || x.Status == Status.Scheduled))
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.SellerId == sellerId)
                .Include(x => x.Item)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.WinnerId == winnerId)
                .Include(x => x.Item)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

    }
}

