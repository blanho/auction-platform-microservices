using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetBidMetrics;

public record GetBidMetricsQuery(AnalyticsQueryParams Query) : IRequest<BidMetrics>;
