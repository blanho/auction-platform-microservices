using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using Common.Core.Helpers;
using Common.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuctionService.Application.Queries.GetUserDashboardStats;

public class GetUserDashboardStatsQueryHandler : IRequestHandler<GetUserDashboardStatsQuery, Result<UserDashboardStatsDto>>
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
            var allAuctions = await _auctionRepository.GetAllAsync(cancellationToken);
            
            var userAuctions = allAuctions.Where(a => a.SellerUsername == request.Username).ToList();
            var wonAuctions = allAuctions.Where(a => a.WinnerUsername == request.Username).ToList();

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
                    Description = $"Listed {auction.Item.Title}",
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
                    Description = $"Won {auction.Item.Title} for ${auction.SoldAmount}",
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
