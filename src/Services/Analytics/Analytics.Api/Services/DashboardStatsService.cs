using Microsoft.EntityFrameworkCore;
using Analytics.Api.Data;
using Analytics.Api.Models;
using Analytics.Api.Grpc;
using Analytics.Api.Interfaces;

namespace Analytics.Api.Services;

public sealed class DashboardStatsService : IDashboardStatsService
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
        var pendingReportsTask = _reportRepository.GetPendingCountAsync(cancellationToken);
        
        Task<AuctionStatsResponse>? auctionStatsTask = null;
        try
        {
            auctionStatsTask = _auctionGrpcClient.GetAuctionStatsAsync(
                new GetAuctionStatsRequest(), 
                cancellationToken: cancellationToken).ResponseAsync;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initiate auction stats gRPC call");
        }

        var stats = new DashboardStats
        {
            PendingReports = await pendingReportsTask
        };

        if (auctionStatsTask is not null)
        {
            try
            {
                var auctionStats = await auctionStatsTask;
                stats.LiveAuctions = auctionStats.LiveAuctions;
                stats.TotalRevenue = (decimal)auctionStats.TotalRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve auction stats via gRPC");
            }
        }

        return stats;
    }

    public async Task<PlatformHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var health = new PlatformHealthStatus
        {
            ApiStatus = HealthStatus.Healthy
        };

        health.DatabaseStatus = await CheckDatabaseHealthAsync(cancellationToken);

        try
        {
            await _auctionGrpcClient.GetAuctionStatsAsync(new GetAuctionStatsRequest(), cancellationToken: cancellationToken);
            health.QueueStatus = HealthStatus.Healthy;
        }
        catch
        {
            health.QueueStatus = HealthStatus.AuctionServiceUnavailable;
        }

        return health;
    }

    private async Task<string> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthStatus.Connected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthStatus.Disconnected;
        }
    }
}
