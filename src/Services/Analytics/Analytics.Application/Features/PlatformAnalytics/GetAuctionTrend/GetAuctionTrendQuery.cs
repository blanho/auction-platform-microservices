using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetAuctionTrend;

public record GetAuctionTrendQuery(DateTimeOffset StartDate, DateTimeOffset EndDate, string Granularity) : IRequest<List<TrendDataPoint>>;
