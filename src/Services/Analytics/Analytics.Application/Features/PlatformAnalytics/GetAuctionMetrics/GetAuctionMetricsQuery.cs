using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetAuctionMetrics;

public record GetAuctionMetricsQuery(AnalyticsQueryParams Query) : IRequest<AuctionMetrics>;
