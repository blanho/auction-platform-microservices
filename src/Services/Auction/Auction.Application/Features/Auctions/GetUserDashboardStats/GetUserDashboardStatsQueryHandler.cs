using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS.Queries;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Queries.GetUserDashboardStats;

public class GetUserDashboardStatsQueryHandler : IQueryHandler<GetUserDashboardStatsQuery, UserDashboardStatsDto>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ILogger<GetUserDashboardStatsQueryHandler> _logger;

    public GetUserDashboardStatsQueryHandler(
        IAuctionRepository auctionRepository,
        ILogger<GetUserDashboardStatsQueryHandler> logger)
    {
        _auctionRepository = auctionRepository;
        _logger = logger;
    }

    public async Task<Result<UserDashboardStatsDto>> Handle(
        GetUserDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userAuctions = await _auctionRepository.GetBySellerUsernameAsync(request.Username, cancellationToken);
            var wonAuctions = await _auctionRepository.GetWonByUsernameAsync(request.Username, cancellationToken);

            var activeListings = userAuctions.Count(a => a.Status == Status.Live);
            var totalListings = userAuctions.Count;
            var itemsWon = wonAuctions.Count;
            var totalSpent = wonAuctions.Sum(a => a.SoldAmount ?? 0m);
            var totalEarnings = userAuctions
                .Where(a => a.Status == Status.Finished && a.SoldAmount.HasValue)
                .Sum(a => a.SoldAmount ?? 0m);

            var recentActivity = new List<RecentActivityDto>();

            foreach (var auction in userAuctions.OrderByDescending(a => a.UpdatedAt).Take(5))
            {
                recentActivity.Add(new RecentActivityDto
                {
                    Type = "listing",
                    Description = $"Listed {auction.Item?.Title ?? "Unknown"}",
                    Timestamp = auction.CreatedAt,
                    RelatedEntityId = auction.Id,
                    RelatedEntityType = "Auction"
                });
            }

            foreach (var auction in wonAuctions.OrderByDescending(a => a.UpdatedAt).Take(5))
            {
                recentActivity.Add(new RecentActivityDto
                {
                    Type = "won",
                    Description = $"Won {auction.Item?.Title ?? "Unknown"} for ${auction.SoldAmount}",
                    Timestamp = auction.UpdatedAt ?? auction.CreatedAt,
                    RelatedEntityId = auction.Id,
                    RelatedEntityType = "Auction"
                });
            }

            var stats = new UserDashboardStatsDto
            {
                TotalBids = 0,
                ItemsWon = itemsWon,
                WatchlistCount = 0,
                ActiveListings = activeListings,
                TotalListings = totalListings,
                TotalSpent = totalSpent,
                TotalEarnings = totalEarnings,
                Balance = totalEarnings - totalSpent,
                SellerRating = 4.5m,
                ReviewCount = totalListings > 0 ? totalListings * 2 : 0,
                RecentActivity = recentActivity
                    .OrderByDescending(r => r.Timestamp)
                    .Take(10)
                    .ToList()
            };

            return Result<UserDashboardStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats for user {Username}", request.Username);
            return Result.Failure<UserDashboardStatsDto>(Error.Create("Dashboard.Error", "Failed to retrieve dashboard statistics"));
        }
    }
}

