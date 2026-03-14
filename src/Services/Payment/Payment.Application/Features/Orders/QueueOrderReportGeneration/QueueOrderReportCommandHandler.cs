using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.CQRS.Commands;
using MassTransit;
using PaymentService.Contracts.Commands;

namespace Payment.Application.Features.Orders.QueueOrderReportGeneration;

public class QueueOrderReportCommandHandler : ICommandHandler<QueueOrderReportCommand, BackgroundJobResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<QueueOrderReportCommandHandler> _logger;

    public QueueOrderReportCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<QueueOrderReportCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<BackgroundJobResult>> Handle(
        QueueOrderReportCommand request,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var command = new GenerateOrderReportCommand
        {
            CorrelationId = correlationId,
            RequestedBy = request.RequestedBy,
            ReportType = request.ReportType.ToString(),
            Format = request.Format.ToString(),
            StatusFilter = request.StatusFilter?.ToString(),
            BuyerIdFilter = request.BuyerIdFilter,
            SellerIdFilter = request.SellerIdFilter,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedAt = DateTimeOffset.UtcNow
        };

        await _publishEndpoint.Publish(command, cancellationToken);

        _logger.LogInformation(
            "Queued order report generation job {CorrelationId} - Type: {ReportType}, Format: {Format} for user {RequestedBy}",
            correlationId, request.ReportType, request.Format, request.RequestedBy);

        return Result<BackgroundJobResult>.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: "Queued",
            Message: $"{request.ReportType} report in {request.Format} format has been queued for background processing."));
    }
}
