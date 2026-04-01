using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions.Providers;
namespace Auctions.Application.Features.Auctions.GetTrendingSearches;

public class GetTrendingSearchesQueryHandler : IQueryHandler<GetTrendingSearchesQuery, List<TrendingSearchDto>>
{
    private readonly IAuctionQueryRepository _auctionRepository;
    private readonly IDateTimeProvider _dateTime;

    public GetTrendingSearchesQueryHandler(
        IAuctionQueryRepository auctionRepository,
        IDateTimeProvider dateTime)
    {
        _auctionRepository = auctionRepository;
        _dateTime = dateTime;
    }

    public async Task<Result<List<TrendingSearchDto>>> Handle(GetTrendingSearchesQuery request, CancellationToken cancellationToken)
    {
        var trendingItems = await _auctionRepository.GetTrendingItemsAsync(request.Limit, cancellationToken);

        var results = trendingItems.Select(item => new TrendingSearchDto
        {
            SearchTerm = item.Item?.Title ?? string.Empty,
            Trending = item.CurrentHighBid.HasValue && item.CurrentHighBid > 1000,
            Hot = item.IsFeatured,
            IsNew = (_dateTime.UtcNowOffset - item.CreatedAt).TotalDays <= 7
        }).ToList();

        return Result<List<TrendingSearchDto>>.Success(results);
    }
}

