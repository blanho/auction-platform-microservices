using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.UserAnalytics.GetTrendingSearches;

public class GetTrendingSearchesQueryHandler : IRequestHandler<GetTrendingSearchesQuery, TrendingSearchesResponse>
{
    private readonly IFactAuctionRepository _auctionRepository;

    public GetTrendingSearchesQueryHandler(IFactAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<TrendingSearchesResponse> Handle(GetTrendingSearchesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _auctionRepository.GetCategoryPerformanceAsync(
            DateTimeOffset.UtcNow.AddDays(-7),
            DateTimeOffset.UtcNow,
            cancellationToken);

        var searches = categories
            .Where(c => !string.IsNullOrWhiteSpace(c.CategoryName))
            .OrderByDescending(c => c.BidCount)
            .Take(request.Limit)
            .Select(c => new TrendingSearchDto
            {
                Query = c.CategoryName,
                Count = c.BidCount
            })
            .ToList();

        return new TrendingSearchesResponse { Searches = searches };
    }
}
