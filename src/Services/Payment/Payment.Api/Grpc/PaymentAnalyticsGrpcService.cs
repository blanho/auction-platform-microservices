using Grpc.Core;
using Payment.Api.Grpc;
using Payment.Application.Interfaces;

namespace Payment.Api.Grpc;

public class PaymentAnalyticsGrpcService : PaymentAnalyticsGrpc.PaymentAnalyticsGrpcBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<PaymentAnalyticsGrpcService> _logger;

    public PaymentAnalyticsGrpcService(
        IOrderRepository orderRepository,
        ILogger<PaymentAnalyticsGrpcService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public override async Task<RevenueStatsResponse> GetRevenueStats(
        GetRevenueStatsRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetRevenueStats called");

        DateTimeOffset? startDate = null;
        DateTimeOffset? endDate = null;

        if (!string.IsNullOrEmpty(request.StartDate) && DateTimeOffset.TryParse(request.StartDate, out var start))
            startDate = start;
        if (!string.IsNullOrEmpty(request.EndDate) && DateTimeOffset.TryParse(request.EndDate, out var end))
            endDate = end;

        var stats = await _orderRepository.GetRevenueStatsAsync(startDate, endDate, context.CancellationToken);

        return new RevenueStatsResponse
        {
            TotalRevenue = (double)stats.TotalRevenue,
            TotalPlatformFees = (double)stats.TotalPlatformFees,
            TotalTransactions = stats.TotalTransactions,
            CompletedOrders = stats.CompletedOrders,
            PendingOrders = stats.PendingOrders,
            RefundedOrders = stats.RefundedOrders,
            AverageOrderValue = (double)stats.AverageOrderValue,
            RevenueToday = (double)stats.RevenueToday,
            RevenueThisWeek = (double)stats.RevenueThisWeek,
            RevenueThisMonth = (double)stats.RevenueThisMonth
        };
    }

    public override async Task<DailyRevenueResponse> GetDailyRevenue(
        GetDailyRevenueRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetDailyRevenue called for {Days} days", request.Days);

        var days = request.Days > 0 ? request.Days : 30;
        var dailyStats = await _orderRepository.GetDailyRevenueAsync(days, context.CancellationToken);

        var response = new DailyRevenueResponse();
        foreach (var stat in dailyStats)
        {
            response.DailyStats.Add(new DailyRevenueStat
            {
                Date = stat.Date.ToString("yyyy-MM-dd"),
                Revenue = (double)stat.Revenue,
                PlatformFees = (double)stat.PlatformFees,
                OrderCount = stat.OrderCount
            });
        }

        return response;
    }

    public override async Task<TopSellersResponse> GetTopSellers(
        GetTopSellersRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetTopSellers called with limit {Limit}", request.Limit);

        var limit = request.Limit > 0 ? request.Limit : 10;
        var period = string.IsNullOrEmpty(request.Period) ? "month" : request.Period;

        var topSellers = await _orderRepository.GetTopSellersAsync(limit, period, context.CancellationToken);

        var response = new TopSellersResponse();
        foreach (var seller in topSellers)
        {
            response.Sellers.Add(new TopSeller
            {
                SellerId = seller.SellerId.ToString(),
                Username = seller.Username,
                TotalSales = (double)seller.TotalSales,
                OrderCount = seller.OrderCount,
                AverageOrderValue = (double)seller.AverageOrderValue
            });
        }

        return response;
    }

    public override async Task<TopBuyersResponse> GetTopBuyers(
        GetTopBuyersRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetTopBuyers called with limit {Limit}", request.Limit);

        var limit = request.Limit > 0 ? request.Limit : 10;
        var period = string.IsNullOrEmpty(request.Period) ? "month" : request.Period;

        var topBuyers = await _orderRepository.GetTopBuyersAsync(limit, period, context.CancellationToken);

        var response = new TopBuyersResponse();
        foreach (var buyer in topBuyers)
        {
            response.Buyers.Add(new TopBuyer
            {
                BuyerId = buyer.BuyerId.ToString(),
                Username = buyer.Username,
                TotalSpent = (double)buyer.TotalSpent,
                OrderCount = buyer.OrderCount
            });
        }

        return response;
    }

    public override async Task<PaymentStatsResponse> GetPaymentStats(
        GetPaymentStatsRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetPaymentStats called");

        DateTimeOffset? startDate = null;
        DateTimeOffset? endDate = null;

        if (!string.IsNullOrEmpty(request.StartDate) && DateTimeOffset.TryParse(request.StartDate, out var start))
            startDate = start;
        if (!string.IsNullOrEmpty(request.EndDate) && DateTimeOffset.TryParse(request.EndDate, out var end))
            endDate = end;

        var stats = await _orderRepository.GetRevenueStatsAsync(startDate, endDate, context.CancellationToken);

        var successRate = stats.TotalTransactions > 0 
            ? (double)stats.CompletedOrders / stats.TotalTransactions * 100 
            : 0;

        return new PaymentStatsResponse
        {
            TotalPayments = stats.TotalTransactions,
            SuccessfulPayments = stats.CompletedOrders,
            FailedPayments = 0,
            PendingPayments = stats.PendingOrders,
            SuccessRate = successRate,
            TotalProcessed = (double)stats.TotalRevenue
        };
    }
}
