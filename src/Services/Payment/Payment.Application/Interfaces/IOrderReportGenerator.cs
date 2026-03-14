using Payment.Application.Features.Orders.QueueOrderReportGeneration;
using Payment.Domain.Enums;

namespace Payment.Application.Interfaces;

public interface IOrderReportGenerator
{
    Task<OrderReportResult> GenerateReportAsync(
        ReportType reportType,
        ReportFormat format,
        OrderReportParameters parameters,
        CancellationToken cancellationToken = default);
}

public record OrderReportParameters(
    OrderStatus? StatusFilter = null,
    Guid? BuyerIdFilter = null,
    Guid? SellerIdFilter = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null);

public record OrderReportResult(
    bool Success,
    string FileName,
    string ContentType,
    byte[] Content,
    long FileSizeBytes,
    int TotalRecords,
    string? ErrorMessage = null);
