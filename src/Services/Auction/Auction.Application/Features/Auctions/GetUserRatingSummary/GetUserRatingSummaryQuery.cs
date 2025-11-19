using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetUserRatingSummary;

public record GetUserRatingSummaryQuery(string Username) : IQuery<UserRatingSummaryDto>;

