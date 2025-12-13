using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using Common.CQRS.Abstractions;
using Common.Core.Helpers;

namespace AuctionService.Application.Queries.GetTrendingSearches;

public class GetTrendingSearchesQueryHandler : IQueryHandler<GetTrendingSearchesQuery, List<TrendingSearchDto>>
{
    private readonly IAuctionRepository _auctionRepository;

    public GetTrendingSearchesQueryHandler(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<Result<List<TrendingSearchDto>>> Handle(GetTrendingSearchesQuery request, CancellationToken cancellationToken)
    {
        var trendingItems = await _auctionRepository.GetTrendingItemsAsync(request.Limit, cancellationToken);
        
        var results = trendingItems.Select(item => new TrendingSearchDto
        {
            SearchTerm = item.Item?.Title ?? "Item",
            Icon = GetIconForCategory(item.Item?.CategoryId),
            Trending = item.CurrentHighBid.HasValue && item.CurrentHighBid > 1000,
            Hot = item.IsFeatured,
            IsNew = (DateTime.UtcNow - item.CreatedAt).TotalDays <= 7,
            Count = 0
        }).ToList();

        return Result<List<TrendingSearchDto>>.Success(results);
    }

    private static string GetIconForCategory(Guid? categoryId)
    {
        return "🔍";
    }
}
