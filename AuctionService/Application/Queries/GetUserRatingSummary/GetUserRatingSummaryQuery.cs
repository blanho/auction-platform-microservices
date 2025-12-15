using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetUserRatingSummary;

public record GetUserRatingSummaryQuery(string Username) : IQuery<UserRatingSummaryDto>;
