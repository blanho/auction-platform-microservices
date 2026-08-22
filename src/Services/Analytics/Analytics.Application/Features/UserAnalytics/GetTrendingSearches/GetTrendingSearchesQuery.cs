using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.UserAnalytics.GetTrendingSearches;

public record GetTrendingSearchesQuery(int Limit) : IRequest<TrendingSearchesResponse>;
