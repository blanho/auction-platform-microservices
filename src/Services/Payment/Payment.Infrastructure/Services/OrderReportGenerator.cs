using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Payment.Application.Features.Orders.QueueOrderReportGeneration;
using Payment.Application.Filtering;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Services;

public class OrderReportGenerator : IOrderReportGenerator
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderReportGenerator> _logger;

    public OrderReportGenerator(
        IOrderRepository orderRepository,
        ILogger<OrderReportGenerator> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<OrderReportResult> GenerateReportAsync(
        ReportType reportType,
        ReportFormat format,
        OrderReportParameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new OrderQueryParams
            {
                Page = 1,
                PageSize = 10000,
                Filter = new OrderFilter
                {
                    Status = parameters.StatusFilter,
                    FromDate = parameters.StartDate?.DateTime,
                    ToDate = parameters.EndDate?.DateTime
                }
            };

            var orders = await _orderRepository.GetAllAsync(queryParams, cancellationToken);
            
            var filteredOrders = orders.Items.AsEnumerable();
            
            if (parameters.BuyerIdFilter.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.BuyerId == parameters.BuyerIdFilter.Value);
            }
            
            if (parameters.SellerIdFilter.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.SellerId == parameters.SellerIdFilter.Value);
            }

            var orderList = filteredOrders.ToList();

            var (content, contentType, extension) = format switch
            {
                ReportFormat.Csv => GenerateCsvReport(orderList, reportType),
                ReportFormat.Excel => GenerateCsvReport(orderList, reportType),
                ReportFormat.Pdf => GeneratePdfReport(orderList, reportType),
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };

            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
            var fileName = $"order-report-{reportType.ToString().ToLowerInvariant()}-{timestamp}{extension}";

            _logger.LogInformation(
                "Generated {ReportType} report with {RecordCount} records, size {Size} bytes",
                reportType, orderList.Count, content.Length);

            return new OrderReportResult(
                Success: true,
                FileName: fileName,
                ContentType: contentType,
                Content: content,
                FileSizeBytes: content.Length,
                TotalRecords: orderList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate {ReportType} report", reportType);
            return new OrderReportResult(
                Success: false,
                FileName: string.Empty,
                ContentType: string.Empty,
                Content: [],
                FileSizeBytes: 0,
                TotalRecords: 0,
                ErrorMessage: ex.Message);
        }
    }

    private static (byte[] Content, string ContentType, string Extension) GenerateCsvReport(
        IReadOnlyCollection<Order> orders,
        ReportType reportType)
    {
        var sb = new StringBuilder();

        sb.AppendLine(reportType switch
        {
            ReportType.OrderSummary => "OrderId,AuctionId,Buyer,Seller,ItemTitle,Status,TotalAmount,CreatedAt",
            ReportType.PaymentTransactions => "OrderId,Buyer,TotalAmount,PaymentStatus,PaymentTransactionId,PaidAt",
            ReportType.SellerPayout => "SellerId,SellerUsername,OrderCount,TotalRevenue,TotalPlatformFees,NetPayout",
            ReportType.BuyerPurchaseHistory => "BuyerId,BuyerUsername,OrderId,ItemTitle,TotalAmount,Status,PaidAt",
            ReportType.RevenueReport => "Date,OrderCount,TotalRevenue,TotalPlatformFees,TotalShipping",
            ReportType.RefundReport => "OrderId,Buyer,Seller,TotalAmount,Status,RefundReason",
            _ => "OrderId,Status,TotalAmount"
        });

        foreach (var order in orders)
        {
            var line = reportType switch
            {
                ReportType.OrderSummary => FormatOrderSummaryRow(order),
                ReportType.PaymentTransactions => FormatPaymentTransactionRow(order),
                ReportType.BuyerPurchaseHistory => FormatBuyerHistoryRow(order),
                _ => FormatOrderSummaryRow(order)
            };
            sb.AppendLine(line);
        }

        return (Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", ".csv");
    }

    private static (byte[] Content, string ContentType, string Extension) GeneratePdfReport(
        IReadOnlyCollection<Order> orders,
        ReportType reportType)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Order Report: {reportType}");
        sb.AppendLine($"Generated: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"Total Records: {orders.Count}");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine();

        foreach (var order in orders)
        {
            sb.AppendLine($"Order ID: {order.Id}");
            sb.AppendLine($"  Buyer: {order.BuyerUsername}");
            sb.AppendLine($"  Seller: {order.SellerUsername}");
            sb.AppendLine($"  Item: {order.ItemTitle}");
            sb.AppendLine($"  Total: ${order.TotalAmount:N2}");
            sb.AppendLine($"  Status: {order.Status}");
            sb.AppendLine($"  Created: {order.CreatedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
        }

        return (Encoding.UTF8.GetBytes(sb.ToString()), "application/pdf", ".pdf");
    }

    private static string FormatOrderSummaryRow(Order order)
    {
        return string.Join(",",
            order.Id,
            order.AuctionId,
            EscapeCsv(order.BuyerUsername),
            EscapeCsv(order.SellerUsername),
            EscapeCsv(order.ItemTitle),
            order.Status,
            order.TotalAmount.ToString(CultureInfo.InvariantCulture),
            order.CreatedAt.ToString("o"));
    }

    private static string FormatPaymentTransactionRow(Order order)
    {
        return string.Join(",",
            order.Id,
            EscapeCsv(order.BuyerUsername),
            order.TotalAmount.ToString(CultureInfo.InvariantCulture),
            order.PaymentStatus,
            order.PaymentTransactionId ?? string.Empty,
            order.PaidAt?.ToString("o") ?? string.Empty);
    }

    private static string FormatBuyerHistoryRow(Order order)
    {
        return string.Join(",",
            order.BuyerId,
            EscapeCsv(order.BuyerUsername),
            order.Id,
            EscapeCsv(order.ItemTitle),
            order.TotalAmount.ToString(CultureInfo.InvariantCulture),
            order.Status,
            order.PaidAt?.ToString("o") ?? string.Empty);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
