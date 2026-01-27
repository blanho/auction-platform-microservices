#nullable enable
using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Infrastructure.Persistence.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTimeProvider _dateTime;
        private readonly IAuditContext _auditContext;

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

        public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var auction = await _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            return auction ?? throw new KeyNotFoundException($"Auction with ID {id} not found");
        }

        public async Task<Auction> CreateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.CreatedAt = _dateTime.UtcNow;
            auction.CreatedBy = _auditContext.UserId;
            auction.IsDeleted = false;
            
            if (auction.Item != null)
            {
                auction.Item.CreatedAt = _dateTime.UtcNow;
                auction.Item.CreatedBy = _auditContext.UserId;
                auction.Item.IsDeleted = false;
            }
            
            await _context.Auctions.AddAsync(auction, cancellationToken);
            
            return auction;
        }

        public async Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.UpdatedAt = _dateTime.UtcNow;
            auction.UpdatedBy = _auditContext.UserId;
            
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
                auction.DeletedBy = _auditContext.UserId;
                
                _context.Auctions.Update(auction);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public Task AddRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            foreach (var auction in auctions)
            {
                auction.CreatedAt = utcNow;
                auction.CreatedBy = _auditContext.UserId;
                auction.IsDeleted = false;

                if (auction.Item != null)
                {
                    auction.Item.CreatedAt = utcNow;
                    auction.Item.CreatedBy = _auditContext.UserId;
                    auction.Item.IsDeleted = false;
                }
            }
            _context.Auctions.AddRange(auctions);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            foreach (var auction in auctions)
            {
                auction.UpdatedAt = utcNow;
                auction.UpdatedBy = _auditContext.UserId;
            }
            _context.Auctions.UpdateRange(auctions);
            return Task.CompletedTask;
        }

        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            var auctions = await _context.Auctions
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var auction in auctions)
            {
                auction.IsDeleted = true;
                auction.DeletedAt = utcNow;
                auction.DeletedBy = _auditContext.UserId;
            }
            _context.Auctions.UpdateRange(auctions);
        }

        public async Task<(List<Auction> Items, int TotalCount)> GetPagedAsync(
            string? status = null,
            string? seller = null,
            string? winner = null,
            string? searchTerm = null,
            string? category = null,
            bool? isFeatured = null,
            string? orderBy = null,
            bool descending = true,
            int pageNumber = PaginationDefaults.DefaultPage,
            int pageSize = PaginationDefaults.DefaultPageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<Status>(status, true, out var statusEnum))
            {
                query = query.Where(x => x.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(seller))
            {
                query = query.Where(x => x.SellerUsername == seller);
            }

            if (!string.IsNullOrEmpty(winner))
            {
                query = query.Where(x => x.WinnerUsername == winner);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(x => 
                    x.Item != null && (
                        x.Item.Title.ToLower().Contains(term) ||
                        (x.Item.Description != null && x.Item.Description.ToLower().Contains(term))
                    ));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(x => x.Item != null && x.Item.Category != null && x.Item.Category.Name == category);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(x => x.IsFeatured == isFeatured.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = orderBy?.ToLower() switch
            {
                "price" => descending 
                    ? query.OrderByDescending(x => x.CurrentHighBid ?? x.ReservePrice)
                    : query.OrderBy(x => x.CurrentHighBid ?? x.ReservePrice),
                "enddate" => descending 
                    ? query.OrderByDescending(x => x.AuctionEnd)
                    : query.OrderBy(x => x.AuctionEnd),
                "createdat" => descending 
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt),
                "title" => descending 
                    ? query.OrderByDescending(x => x.Item != null ? x.Item.Title : string.Empty)
                    : query.OrderBy(x => x.Item != null ? x.Item.Title : string.Empty),
                _ => descending 
                    ? query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                    : query.OrderBy(x => x.UpdatedAt ?? x.CreatedAt)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
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

        public async Task<int> GetCountByStatusAsync(Status status, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.Status == status)
                .CountAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted)
                .CountAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.Status == Status.Finished && x.SoldAmount.HasValue)
                .SumAsync(x => x.SoldAmount ?? 0, cancellationToken);
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

        public async Task<int> GetCountEndingBetweenAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted 
                    && x.Status == Status.Live
                    && x.AuctionEnd >= start 
                    && x.AuctionEnd < end)
                .CountAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetTopByRevenueAsync(int limit, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.Status == Status.Finished && x.SoldAmount.HasValue)
                .Include(x => x.Item)
                .AsNoTracking()
                .OrderByDescending(x => x.SoldAmount)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryStatDto>> GetCategoryStatsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.Item != null && x.Item.CategoryId.HasValue)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .AsNoTracking()
                .GroupBy(x => new { x.Item!.CategoryId, CategoryName = x.Item.Category!.Name })
                .Select(g => new CategoryStatDto(
                    g.Key.CategoryId!.Value,
                    g.Key.CategoryName,
                    g.Count(),
                    g.Sum(x => x.SoldAmount ?? 0)
                ))
                .OrderByDescending(c => c.Revenue)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetBySellerUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.SellerUsername == username)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetWonByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted && x.WinnerUsername == username)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Category)
                .Include(x => x.Item)
                    .ThenInclude(i => i!.Brand)
                .AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<SellerStatsDto> GetSellerStatsAsync(
            string username, 
            DateTimeOffset periodStart, 
            DateTimeOffset? previousPeriodStart = null, 
            CancellationToken cancellationToken = default)
        {
            var userAuctions = await _context.Auctions
                .Where(x => !x.IsDeleted && x.SellerUsername == username)
                .Include(x => x.Item)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var currentPeriodSales = userAuctions
                .Where(a => a.Status == Status.Finished && 
                           a.SoldAmount.HasValue && 
                           a.UpdatedAt >= periodStart)
                .ToList();

            var previousPeriodSales = previousPeriodStart.HasValue
                ? userAuctions
                    .Where(a => a.Status == Status.Finished && 
                               a.SoldAmount.HasValue && 
                               a.UpdatedAt >= previousPeriodStart.Value &&
                               a.UpdatedAt < periodStart)
                    .ToList()
                : new List<Auction>();

            return new SellerStatsDto
            {
                TotalRevenue = currentPeriodSales.Sum(a => a.SoldAmount ?? 0),
                PreviousPeriodRevenue = previousPeriodSales.Sum(a => a.SoldAmount ?? 0),
                ItemsSold = currentPeriodSales.Count,
                PreviousPeriodItemsSold = previousPeriodSales.Count,
                ActiveListings = userAuctions.Count(a => a.Status == Status.Live),
                TotalListings = userAuctions.Count,
                PendingAuctions = userAuctions.Count(a => a.Status == Status.Scheduled),
                DraftAuctions = userAuctions.Count(a => a.Status == Status.Draft),
                RecentSales = currentPeriodSales
                    .OrderByDescending(a => a.UpdatedAt)
                    .Take(10)
                    .Select(a => new AuctionSummaryDto
                    {
                        Id = a.Id,
                        Title = a.Item?.Title ?? "Unknown",
                        SoldAmount = a.SoldAmount,
                        SoldAt = a.UpdatedAt,
                        BuyerUsername = a.WinnerUsername
                    })
                    .ToList()
            };
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

        public async Task<int> GetWatchlistCountByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Bookmarks
                .Where(b => b.Username == username && !b.IsDeleted)
                .CountAsync(cancellationToken);
        }
    }
}

