#nullable enable
using Auctions.Application.Filtering;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Extensions;
using System.Linq.Expressions;
using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Paging;
using Auctions.Domain.Enums;

namespace Auctions.Infrastructure.Persistence.Repositories
{
    public class AuctionRepository : EfRepository<Auction, AuctionDbContext>, IAuctionReadRepository, IAuctionWriteRepository
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<Auction, object>>> AuctionSortMap =
            new SortMapBuilder<Auction, AuctionSortFields>()
                .Add(field => field.Price, x => x.CurrentHighBid ?? x.ReservePrice)
                .Add(field => field.EndDate, x => x.AuctionEnd)
                .Add(field => field.CreatedAt, x => x.CreatedAt)
                .Add(field => field.Title, x => x.Item != null ? x.Item.Title : string.Empty)
                .Build();

        private IQueryable<Auction> ActiveAuctionsWithItemDetails =>
            Context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item);

        protected override IQueryable<Auction> Query => ActiveAuctionsWithItemDetails;

        protected override void ApplyCreatedAudit(Auction auction, DateTimeOffset timestamp)
        {
            base.ApplyCreatedAudit(auction, timestamp);
            auction.Item?.SetCreatedAudit(GetCreatedBy(auction), timestamp);
        }

        protected override void TrackUpdate(Auction auction)
        {
            if (Context.Entry(auction).State == EntityState.Detached)
                Context.Auctions.Update(auction);
        }

        public AuctionRepository(AuctionDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
            : base(context, dateTime, auditContext)
        {
        }

        public override async Task<PaginatedResult<Auction>> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = ActiveAuctionsWithItemDetails
                .AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);

            return await query
                .ThenBy(entity => entity.Id)
                .ToPaginatedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<PaginatedResult<Auction>> GetPagedAsync(
            AuctionFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var filterParams = filter.Filter;

            var query = ActiveAuctionsWithItemDetails
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
                query = query.Where(x => x.Item != null && x.Item.CategoryName == filterParams.Category);
            }

            if (filterParams.IsFeatured.HasValue)
            {
                query = query.Where(x => x.IsFeatured == filterParams.IsFeatured.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = query.ApplySorting(filter, AuctionSortMap, x => x.UpdatedAt ?? x.CreatedAt, tieBreaker: x => x.Id);

            var items = await query
                .ApplyPaging(filter)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Auction>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default)
        {
            var now = Clock.UtcNow;

            return await Context.Auctions
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
            var now = Clock.UtcNow;

            return await Context.Auctions
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
            var query = Context.Auctions
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
            return await Context.Auctions
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
            return await Context.Auctions
                .Where(x => !x.IsDeleted
                    && x.Status == Status.Live
                    && x.AuctionEnd >= startTime
                    && x.AuctionEnd <= endTime)
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken cancellationToken = default)
        {
            return await ActiveAuctionsWithItemDetails
                .Where(x => x.Status == Status.Live)
                .AsNoTracking()
                .OrderByDescending(x => x.CurrentHighBid ?? 0)
                .ThenByDescending(x => x.IsFeatured)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
        {
            return await Context.Auctions
                .Where(x => !x.IsDeleted &&
                           x.SellerId == sellerId &&
                           (x.Status == Status.Live || x.Status == Status.Scheduled))
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
        {
            return await Context.Auctions
                .Where(x => !x.IsDeleted && x.SellerId == sellerId)
                .Include(x => x.Item)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken cancellationToken = default)
        {
            return await Context.Auctions
                .Where(x => !x.IsDeleted && x.WinnerId == winnerId)
                .Include(x => x.Item)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

    }
}
