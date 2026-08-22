using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetRevenueTrend;

public record GetRevenueTrendQuery(DateTimeOffset StartDate, DateTimeOffset EndDate, string Granularity) : IRequest<List<TrendDataPoint>>;
