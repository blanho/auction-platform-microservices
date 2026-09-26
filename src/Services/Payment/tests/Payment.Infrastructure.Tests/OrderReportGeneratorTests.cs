using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application.Features.Orders.QueueOrderReportGeneration;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Services;
using PdfSharp.Pdf.IO;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class OrderReportGeneratorTests
{
    [Fact]
    public async Task GenerateReport_ProducesValidExcelAndPdfFiles()
    {
        var order = CreatePaidOrder();
        var generator = CreateGenerator([order]);

        var excel = await generator.GenerateReportAsync(
            ReportType.OrderSummary, ReportFormat.Excel, new OrderReportParameters());
        var pdf = await generator.GenerateReportAsync(
            ReportType.OrderSummary, ReportFormat.Pdf, new OrderReportParameters());

        Assert.True(excel.Success, excel.ErrorMessage);
        Assert.EndsWith(".xlsx", excel.FileName);
        using (var workbook = new XLWorkbook(new MemoryStream(excel.Content)))
        {
            var sheet = workbook.Worksheet("Orders");
            Assert.Equal("OrderId", sheet.Cell(1, 1).GetString());
            Assert.Equal(order.Id.ToString(), sheet.Cell(2, 1).GetString());
            Assert.Equal(order.TotalAmount, sheet.Cell(2, 7).GetValue<decimal>());
        }

        Assert.True(pdf.Success, pdf.ErrorMessage);
        Assert.EndsWith(".pdf", pdf.FileName);
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf.Content, 0, 5));
        using var document = PdfReader.Open(new MemoryStream(pdf.Content), PdfDocumentOpenMode.Import);
        Assert.True(document.PageCount > 0);
    }

    [Fact]
    public async Task GenerateReport_UsesColumnsAndAggregatesForEachReportType()
    {
        var paid = CreatePaidOrder();
        var refunded = CreatePaidOrder();
        refunded.ChangeStatus(OrderStatus.Disputed);
        refunded.ChangeStatus(OrderStatus.Refunded);
        var generator = CreateGenerator([paid, refunded]);

        var payout = await generator.GenerateReportAsync(
            ReportType.SellerPayout, ReportFormat.Csv, new OrderReportParameters());
        var revenue = await generator.GenerateReportAsync(
            ReportType.RevenueReport, ReportFormat.Csv, new OrderReportParameters());
        var refunds = await generator.GenerateReportAsync(
            ReportType.RefundReport, ReportFormat.Csv, new OrderReportParameters());

        Assert.True(payout.Success);
        Assert.Equal(1, payout.TotalRecords);
        Assert.Contains($",1,{paid.TotalAmount},", Encoding.UTF8.GetString(payout.Content));
        Assert.True(revenue.Success);
        Assert.Equal(1, revenue.TotalRecords);
        Assert.StartsWith("Date,OrderCount,TotalRevenue,TotalPlatformFees,TotalShipping", Encoding.UTF8.GetString(revenue.Content));
        Assert.True(refunds.Success);
        Assert.Equal(1, refunds.TotalRecords);
        Assert.Contains(refunded.Id.ToString(), Encoding.UTF8.GetString(refunds.Content));
        Assert.DoesNotContain(paid.Id.ToString(), Encoding.UTF8.GetString(refunds.Content));
    }

    [Fact]
    public async Task GenerateReport_CsvRowsMatchHeadersForEveryReportType()
    {
        var paid = CreatePaidOrder();
        var refunded = CreatePaidOrder();
        refunded.ChangeStatus(OrderStatus.Disputed);
        refunded.ChangeStatus(OrderStatus.Refunded);
        var generator = CreateGenerator([paid, refunded]);

        foreach (var reportType in Enum.GetValues<ReportType>())
        {
            var result = await generator.GenerateReportAsync(
                reportType, ReportFormat.Csv, new OrderReportParameters());

            Assert.True(result.Success, result.ErrorMessage);
            var lines = Encoding.UTF8.GetString(result.Content)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var columnCount = lines[0].Split(',').Length;
            Assert.All(lines.Skip(1), line => Assert.Equal(columnCount, line.TrimEnd('\r').Split(',').Length));
        }
    }

    private static Order CreatePaidOrder()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "buyer", Guid.NewGuid(),
            "seller", "Đồng hồ", 100m, platformFeePercent: 10m, shippingCost: 5m);
        order.CompletePayment(Guid.NewGuid().ToString());
        return order;
    }

    private static OrderReportGenerator CreateGenerator(List<Order> orders)
    {
        var repository = DispatchProxy.Create<IOrderRepository, OrderRepositoryProxy>();
        ((OrderRepositoryProxy)(object)repository).Orders = orders;
        return new OrderReportGenerator(repository, NullLogger<OrderReportGenerator>.Instance);
    }

    public class OrderRepositoryProxy : DispatchProxy
    {
        public List<Order> Orders { get; set; } = [];

        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            method?.Name == nameof(IOrderRepository.GetForReportAsync)
                ? Task.FromResult(Orders)
                : throw new NotSupportedException(method?.Name);
    }
}
