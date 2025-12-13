using AuctionService.Application.DTOs;
using Common.Core.Helpers;
using MediatR;

namespace AuctionService.Application.Queries.GetSellerAnalytics;

public record GetSellerAnalyticsQuery(string Username, string TimeRange) : IRequest<Result<SellerAnalyticsDto>>;
