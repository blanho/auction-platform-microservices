using Auctions.Application.DTOs;
using MediatR;

namespace Auctions.Application.Queries.GetSellerAnalytics;

public record GetSellerAnalyticsQuery(string Username, string TimeRange) : IRequest<Result<SellerAnalyticsDto>>;

