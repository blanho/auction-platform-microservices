using Microsoft.EntityFrameworkCore;
using AnalyticsService.Data;
using AnalyticsService.Grpc;
using AnalyticsService.Interfaces;

namespace AnalyticsService.Services;

public class DashboardStatsService : IDashboardStatsService
{
    private readonly AnalyticsDbContext _context;
    private readonly IReportRepository _reportRepository;
    private readonly AuctionGrpc.AuctionGrpcClient _auctionGrpcClient;
    private readonly ILogger<DashboardStatsService> _logger;

    public DashboardStatsService(
        AnalyticsDbContext context,
        IReportRepository reportRepository,
        AuctionGrpc.AuctionGrpcClient auctionGrpcClient,
        ILogger<DashboardStatsService> logger)
    {
        _context = context;
        _reportRepository = reportRepository;
        _auctionGrpcClient = auctionGrpcClient;
        _logger = logger;
    }

    public async Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var pendingReports = await _reportRepository.GetPendingCountAsync(cancellationToken);

        var stats = new DashboardStats
        {
            PendingReports = pendingReports
        };

        try
        {
            var auctionStats = await _auctionGrpcClient.GetAuctionStatsAsync(
                new GetAuctionStatsRequest(), 
                cancellationToken: cancellationToken);

            stats.LiveAuctions = auctionStats.LiveAuctions;
            stats.TotalRevenue = (decimal)auctionStats.TotalRevenue;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve auction stats via gRPC");
        }

        return stats;
    }

    public async Task<PlatformHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var health = new PlatformHealthStatus
        {
            ApiStatus = "healthy"
        };

        health.DatabaseStatus = await CheckDatabaseHealthAsync(cancellationToken);

        try
        {
            await _auctionGrpcClient.GetAuctionStatsAsync(new GetAuctionStatsRequest(), cancellationToken: cancellationToken);
            health.QueueStatus = "healthy";
        }
        catch
        {
            health.QueueStatus = "auction_service_unavailable";
        }

        return health;
    }

    private async Task<string> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return "connected";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return "disconnected";
        }
    }
}
