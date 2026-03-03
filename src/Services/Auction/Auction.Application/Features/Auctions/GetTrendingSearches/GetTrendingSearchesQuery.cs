using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Auctions.GetTrendingSearches;

public record GetTrendingSearchesQuery(int Limit) : IQuery<List<TrendingSearchDto>>;

