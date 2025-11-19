using Auctions.Application.DTOs;

namespace Auctions.Application.Queries.GetTrendingSearches;

public record GetTrendingSearchesQuery(int Limit) : IQuery<List<TrendingSearchDto>>;

