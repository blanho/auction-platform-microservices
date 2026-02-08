using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetTrendingSearches;

public class GetTrendingSearchesQueryHandler : IQueryHandler<GetTrendingSearchesQuery, List<TrendingSearchDto>>
{
    private readonly IAuctionReadRepository _auctionRepository;

    public GetTrendingSearchesQueryHandler(IAuctionReadRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<Result<List<TrendingSearchDto>>> Handle(GetTrendingSearchesQuery request, CancellationToken cancellationToken)
    {
        var trendingItems = await _auctionRepository.GetTrendingItemsAsync(request.Limit, cancellationToken);
        
        var results = trendingItems.Select(item => new TrendingSearchDto
        {
            SearchTerm = item.Item?.Title ?? "Item",
            Icon = GetIconForCategory(),
            Trending = item.CurrentHighBid.HasValue && item.CurrentHighBid > 1000,
            Hot = item.IsFeatured,
            IsNew = (DateTime.UtcNow - item.CreatedAt).TotalDays <= 7,
            Count = 0
        }).ToList();

        return Result<List<TrendingSearchDto>>.Success(results);
    }

    private static string GetIconForCategory()
    {
        return "🔍";
    }
}

