using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS.Queries;

namespace Auctions.Application.Queries.GetSellerAnalytics;

public record GetSellerAnalyticsQuery(string Username, string TimeRange) : IQuery<SellerAnalyticsDto>;

