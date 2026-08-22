using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetTopPerformers;

public record GetTopPerformersQuery(int Limit, string Period) : IRequest<TopPerformersDto>;
