using Common.CQRS.Abstractions;
using AuctionService.Application.DTOs;

namespace AuctionService.Application.Queries.GetTrendingSearches;

public record GetTrendingSearchesQuery(int Limit) : IQuery<List<TrendingSearchDto>>;
