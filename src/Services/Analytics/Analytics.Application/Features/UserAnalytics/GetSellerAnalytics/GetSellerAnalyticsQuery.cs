using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.UserAnalytics.GetSellerAnalytics;

public record GetSellerAnalyticsQuery(string Username, string TimeRange) : IRequest<SellerAnalyticsDto>;
