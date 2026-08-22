using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetCategoryPerformance;

public record GetCategoryPerformanceQuery(DateTimeOffset? StartDate, DateTimeOffset? EndDate) : IRequest<List<CategoryBreakdown>>;
