using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetRevenueMetrics;

public record GetRevenueMetricsQuery(AnalyticsQueryParams Query) : IRequest<RevenueMetrics>;
