using Microsoft.EntityFrameworkCore;
using Analytics.Api.Data;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;

namespace Analytics.Api.Services;

public sealed class DashboardStatsService : IDashboardStatsService
{
    private readonly AnalyticsDbContext _context;
    private readonly IReportRepository _reportRepository;
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactPaymentRepository _paymentRepository;
    private readonly ILogger<DashboardStatsService> _logger;

    public DashboardStatsService(
        AnalyticsDbContext context,
        IReportRepository reportRepository,
        IFactAuctionRepository auctionRepository,
        IFactPaymentRepository paymentRepository,
        ILogger<DashboardStatsService> logger)
    {
        _context = context;
        _reportRepository = reportRepository;
        _auctionRepository = auctionRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var pendingReportsTask = _reportRepository.GetPendingCountAsync(cancellationToken);
        var liveAuctionsTask = _auctionRepository.GetLiveAuctionsCountAsync(cancellationToken);
        var revenueTask = _paymentRepository.GetRevenueMetricsAsync(null, null, cancellationToken);

        await Task.WhenAll(pendingReportsTask, liveAuctionsTask, revenueTask);

        var revenueMetrics = await revenueTask;

        return new DashboardStats
        {
            PendingReports = await pendingReportsTask,
            LiveAuctions = await liveAuctionsTask,
            TotalRevenue = revenueMetrics.TotalRevenue,
            TotalOrders = revenueMetrics.TotalTransactions,
            CompletedOrders = revenueMetrics.CompletedOrders
        };
    }

    public async Task<PlatformHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var health = new PlatformHealthStatus
        {
            ApiStatus = HealthStatus.Healthy
        };

        health.DatabaseStatus = await CheckDatabaseHealthAsync(cancellationToken);
        health.QueueStatus = HealthStatus.Healthy;

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
