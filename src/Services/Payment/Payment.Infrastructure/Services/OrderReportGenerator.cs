using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Payment.Application.Features.Orders.QueueOrderReportGeneration;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace Payment.Infrastructure.Services;

public class OrderReportGenerator : IOrderReportGenerator
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private static readonly object FontRegistrationLock = new();
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderReportGenerator> _logger;

    public OrderReportGenerator(IOrderRepository orderRepository, ILogger<OrderReportGenerator> logger)
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
            var orders = await _orderRepository.GetForReportAsync(parameters, cancellationToken);
            var report = BuildReport(orders, reportType);
            var (content, contentType, extension) = format switch
            {
                ReportFormat.Csv => (GenerateCsv(report), "text/csv", ".csv"),
                ReportFormat.Excel => (GenerateExcel(report), ExcelContentType, ".xlsx"),
                ReportFormat.Pdf => (GeneratePdf(report, reportType), "application/pdf", ".pdf"),
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };

            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
            var fileName = $"order-report-{reportType.ToString().ToLowerInvariant()}-{timestamp}{extension}";
            _logger.LogInformation(
                "Generated {ReportType} report with {RecordCount} records, size {Size} bytes",
                reportType, report.Rows.Count, content.Length);

            return new OrderReportResult(true, fileName, contentType, content, content.Length, report.Rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate {ReportType} report", reportType);
            return new OrderReportResult(false, string.Empty, string.Empty, [], 0, 0, ex.Message);
        }
    }

    private static ReportTable BuildReport(IReadOnlyList<Order> orders, ReportType reportType) => reportType switch
    {
        ReportType.OrderSummary => new(
            ["OrderId", "AuctionId", "Buyer", "Seller", "ItemTitle", "Status", "TotalAmount", "CreatedAt"],
            orders.Select(o => new object?[]
            {
                o.Id, o.AuctionId, o.BuyerUsername, o.SellerUsername, o.ItemTitle,
                o.Status, o.TotalAmount, o.CreatedAt
            }).ToList()),
        ReportType.PaymentTransactions => new(
            ["OrderId", "Buyer", "TotalAmount", "PaymentStatus", "PaymentTransactionId", "PaidAt"],
            orders.Select(o => new object?[]
            {
                o.Id, o.BuyerUsername, o.TotalAmount, o.PaymentStatus, o.PaymentTransactionId, o.PaidAt
            }).ToList()),
        ReportType.SellerPayout => BuildSellerPayoutReport(orders),
        ReportType.BuyerPurchaseHistory => new(
            ["BuyerId", "BuyerUsername", "OrderId", "ItemTitle", "TotalAmount", "Status", "PaidAt"],
            orders.Select(o => new object?[]
            {
                o.BuyerId, o.BuyerUsername, o.Id, o.ItemTitle, o.TotalAmount, o.Status, o.PaidAt
            }).ToList()),
        ReportType.RevenueReport => BuildRevenueReport(orders),
        ReportType.RefundReport => new(
            ["OrderId", "Buyer", "Seller", "TotalAmount", "Status"],
            orders.Where(o => o.Status == OrderStatus.Refunded)
                .Select(o => new object?[]
                {
                    o.Id, o.BuyerUsername, o.SellerUsername, o.TotalAmount, o.Status
                }).ToList()),
        _ => new(
            ["OrderId", "Status", "TotalAmount"],
            orders.Select(o => new object?[] { o.Id, o.Status, o.TotalAmount }).ToList())
    };

    private static ReportTable BuildSellerPayoutReport(IReadOnlyList<Order> orders)
    {
        var rows = orders.Where(IsPaidAndNotRefunded)
            .GroupBy(o => new { o.SellerId, o.SellerUsername })
            .OrderBy(group => group.Key.SellerUsername)
            .Select(group =>
            {
                var revenue = group.Sum(o => o.TotalAmount);
                var fees = group.Sum(o => o.PlatformFee ?? 0);
                return new object?[]
                {
                    group.Key.SellerId, group.Key.SellerUsername, group.Count(), revenue, fees, revenue - fees
                };
            }).ToList();

        return new ReportTable(
            ["SellerId", "SellerUsername", "OrderCount", "TotalRevenue", "TotalPlatformFees", "NetPayout"], rows);
    }

    private static ReportTable BuildRevenueReport(IReadOnlyList<Order> orders)
    {
        var rows = orders.Where(IsPaidAndNotRefunded)
            .GroupBy(o => DateOnly.FromDateTime(o.PaidAt!.Value.UtcDateTime))
            .OrderBy(group => group.Key)
            .Select(group => new object?[]
            {
                group.Key, group.Count(), group.Sum(o => o.TotalAmount),
                group.Sum(o => o.PlatformFee ?? 0), group.Sum(o => o.ShippingCost ?? 0)
            }).ToList();

        return new ReportTable(
            ["Date", "OrderCount", "TotalRevenue", "TotalPlatformFees", "TotalShipping"], rows);
    }

    private static bool IsPaidAndNotRefunded(Order order) =>
        order.PaymentStatus == PaymentStatus.Completed &&
        order.Status != OrderStatus.Refunded && order.PaidAt.HasValue;

    private static byte[] GenerateCsv(ReportTable report)
    {
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(',', report.Headers));
        foreach (var row in report.Rows)
            csv.AppendLine(string.Join(',', row.Select(value => EscapeCsv(FormatValue(value)))));
        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static byte[] GenerateExcel(ReportTable report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Orders");
        for (var column = 0; column < report.Headers.Length; column++)
            sheet.Cell(1, column + 1).Value = report.Headers[column];

        for (var row = 0; row < report.Rows.Count; row++)
        {
            for (var column = 0; column < report.Headers.Length; column++)
            {
                var cell = sheet.Cell(row + 2, column + 1);
                var value = report.Rows[row][column];
                if (value is decimal amount)
                    cell.Value = amount;
                else if (value is int count)
                    cell.Value = count;
                else
                    cell.Value = FormatValue(value);
            }
        }

        sheet.Row(1).Style.Font.Bold = true;
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] GeneratePdf(ReportTable report, ReportType reportType)
    {
        RegisterPdfFont();
        using var document = new PdfDocument();
        document.Info.Title = $"Order Report: {reportType}";
        var font = new XFont("Lato", 10);
        var titleFont = new XFont("Lato", 14);
        var page = document.AddPage();
        page.Size = PdfSharp.PageSize.A4;
        var graphics = XGraphics.FromPdfPage(page);
        const double margin = 40;
        const double lineHeight = 15;
        var y = margin;

        void WriteLine(string line, XFont lineFont)
        {
            if (y + lineHeight > page.Height.Point - margin)
            {
                graphics.Dispose();
                page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                graphics = XGraphics.FromPdfPage(page);
                y = margin;
            }
            graphics.DrawString(line, lineFont, XBrushes.Black, margin, y);
            y += lineHeight;
        }

        WriteLine($"Order Report: {reportType}", titleFont);
        WriteLine($"Total Records: {report.Rows.Count}", font);
        y += lineHeight;
        foreach (var row in report.Rows)
        {
            for (var column = 0; column < report.Headers.Length; column++)
            {
                var field = $"{report.Headers[column]}: {FormatValue(row[column]).Replace('\r', ' ').Replace('\n', ' ')}";
                foreach (var line in WrapPdfLine(field, graphics, font, page.Width.Point - 2 * margin).ToList())
                    WriteLine(line, font);
            }
            y += lineHeight;
        }

        graphics.Dispose();
        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static IEnumerable<string> WrapPdfLine(string text, XGraphics graphics, XFont font, double maxWidth)
    {
        var line = new StringBuilder();
        foreach (var word in text.Split(' '))
        {
            var candidate = line.Length == 0 ? word : $"{line} {word}";
            if (graphics.MeasureString(candidate, font).Width <= maxWidth)
            {
                line.Clear().Append(candidate);
                continue;
            }
            if (line.Length > 0)
            {
                yield return line.ToString();
                line.Clear();
            }
            foreach (var character in word)
            {
                if (line.Length > 0 && graphics.MeasureString($"{line}{character}", font).Width > maxWidth)
                {
                    yield return line.ToString();
                    line.Clear();
                }
                line.Append(character);
            }
        }
        if (line.Length > 0)
            yield return line.ToString();
    }

    private static void RegisterPdfFont()
    {
        lock (FontRegistrationLock)
        {
            if (GlobalFontSettings.FontResolver is null)
                GlobalFontSettings.FontResolver = new LatoFontResolver();
        }
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTimeOffset date => date.ToString("o", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        IFormattable formatted => formatted.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string EscapeCsv(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private sealed record ReportTable(string[] Headers, List<object?[]> Rows);

    private sealed class LatoFontResolver : IFontResolver
    {
        private const string FaceName = "Lato-Regular";
        private static readonly byte[] FontBytes = LoadFont();

        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic) =>
            familyName == "Lato" ? new FontResolverInfo(FaceName, bold, italic) : null;

        public byte[]? GetFont(string faceName) => faceName == FaceName ? FontBytes : null;

        private static byte[] LoadFont()
        {
            using var stream = typeof(OrderReportGenerator).Assembly.GetManifestResourceStream(
                "Payment.Infrastructure.Fonts.Lato-Regular.ttf")
                ?? throw new InvalidOperationException("The report font is missing.");
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToArray();
        }
    }
}
