using Bidding.Application.DTOs;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Extensions;

using System.Linq.Expressions;
using Bidding.Application.Filtering;
using Bidding.Application.Interfaces;
using Bidding.Domain.Entities;
using Bidding.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Microsoft.EntityFrameworkCore;

namespace Bidding.Infrastructure.Repositories;

public class AutoBidRepository : EfRepository<AutoBid, BidDbContext>, IAutoBidRepository
{
    protected override Guid GetCreatedBy(AutoBid autoBid) => autoBid.UserId;

    protected override Guid GetUpdatedBy(AutoBid autoBid) => autoBid.UserId;

    private static readonly IReadOnlyDictionary<string, Expression<Func<AutoBid, object>>> AutoBidSortMap =
        new SortMapBuilder<AutoBid, AutoBidDto>()
            .Add(field => field.CreatedAt, x => x.CreatedAt)
            .Add(field => field.MaxAmount, x => x.MaxAmount)
            .Add(field => field.CurrentBidAmount, x => x.CurrentBidAmount)
            .Add(field => field.LastBidAt, x => x.LastBidAt)
            .Build();

    public AutoBidRepository(BidDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
        : base(context, dateTime, auditContext)
    {
    }

    public Task<AutoBid?> GetActiveAutoBidAsync(Guid auctionId, Guid userId, CancellationToken cancellationToken = default) =>
        FirstOrDefaultAsync(autoBid => autoBid.AuctionId == auctionId && autoBid.UserId == userId && autoBid.IsActive,
            cancellationToken);

    public async Task<List<AutoBid>> GetActiveAutoBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await Context.AutoBids
            .AsNoTracking()
            .Where(ab => !ab.IsDeleted && ab.AuctionId == auctionId && ab.IsActive)
            .OrderByDescending(ab => ab.MaxAmount)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<AutoBid>> GetAutoBidsByUserAsync(Guid userId, AutoBidQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var filter = queryParams.Filter;

        var filterBuilder = FilterBuilder<AutoBid>.Create()
            .When(true, x => !x.IsDeleted)
            .When(true, x => x.UserId == userId)
            .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
            .WhenHasValue(filter.IsActive, x => x.IsActive == filter.IsActive!.Value)
            .WhenHasValue(filter.MinMaxAmount, x => x.MaxAmount >= filter.MinMaxAmount!.Value)
            .WhenHasValue(filter.MaxMaxAmount, x => x.MaxAmount <= filter.MaxMaxAmount!.Value)
            .WhenHasValue(filter.FromDate, x => x.CreatedAt >= filter.FromDate!.Value)
            .WhenHasValue(filter.ToDate, x => x.CreatedAt <= filter.ToDate!.Value);

        var query = Context.AutoBids
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, AutoBidSortMap, ab => ab.CreatedAt, tieBreaker: ab => ab.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AutoBid>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    public Task<int> GetAutoBidsCountForUserAsync(Guid userId, bool? activeOnly, CancellationToken cancellationToken = default) =>
        CountAsync(autoBid => autoBid.UserId == userId && (!activeOnly.HasValue || autoBid.IsActive == activeOnly),
            cancellationToken);

}
