using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetPlatformAnalytics;

public record GetPlatformAnalyticsQuery(AnalyticsQueryParams Query) : IRequest<PlatformAnalyticsDto>;
