using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.CQRS.Commands;

namespace Payment.Application.Features.Orders.QueueOrderReportGeneration;

public record QueueOrderReportCommand(
    Guid RequestedBy,
    ReportType ReportType,
    ReportFormat Format,
    OrderStatus? StatusFilter = null,
    Guid? BuyerIdFilter = null,
    Guid? SellerIdFilter = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null) : ICommand<BackgroundJobResult>;

public enum ReportType
{
    OrderSummary,
    PaymentTransactions,
    SellerPayout,
    BuyerPurchaseHistory,
    RevenueReport,
    RefundReport
}

public enum ReportFormat
{
    Pdf,
    Excel,
    Csv
}
