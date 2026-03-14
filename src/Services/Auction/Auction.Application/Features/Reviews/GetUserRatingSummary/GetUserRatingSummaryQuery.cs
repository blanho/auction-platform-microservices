using BuildingBlocks.Application.CQRS;
using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Reviews.GetUserRatingSummary;

public record GetUserRatingSummaryQuery(string Username) : IQuery<UserRatingSummaryDto>;
