using System.Diagnostics;
using System.Text.Json;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.Application.Interfaces;
using Payment.Application.Features.Orders.QueueOrderReportGeneration;
using Payment.Domain.Enums;
using PaymentService.Contracts.Commands;
using PaymentService.Contracts.Events;

namespace Payment.Infrastructure.Messaging.Consumers;

public class GenerateOrderReportConsumer : IConsumer<GenerateOrderReportCommand>
{
    private readonly IOrderReportGenerator _reportGenerator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GenerateOrderReportConsumer> _logger;

    public GenerateOrderReportConsumer(
        IOrderReportGenerator reportGenerator,
        IPublishEndpoint publishEndpoint,
        ILogger<GenerateOrderReportConsumer> logger)
    {
        _reportGenerator = reportGenerator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GenerateOrderReportCommand> context)
    {
        var message = context.Message;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing order report generation {CorrelationId} - Type: {ReportType}, Format: {Format}",
            correlationId, message.ReportType, message.Format);

        await PublishJobStarted(context, message);

        var reportType = ParseReportType(message.ReportType);
        var format = ParseReportFormat(message.Format);
        var statusFilter = ParseStatusFilter(message.StatusFilter);

        if (reportType is null || format is null)
        {
            _logger.LogWarning(
                "Invalid report type or format for {CorrelationId}: Type={ReportType}, Format={Format}",
                correlationId, message.ReportType, message.Format);

            await PublishJobFailed(context, correlationId, "Invalid report type or format specified");
            return;
        }

        var parameters = new OrderReportParameters(
            StatusFilter: statusFilter,
            BuyerIdFilter: message.BuyerIdFilter,
            SellerIdFilter: message.SellerIdFilter,
            StartDate: message.StartDate,
            EndDate: message.EndDate);

        var result = await _reportGenerator.GenerateReportAsync(
            reportType.Value, format.Value, parameters, context.CancellationToken);

        stopwatch.Stop();

        if (result.Success)
        {
            _logger.LogInformation(
                "Order report generation completed {CorrelationId} - Records: {TotalRecords}, Size: {FileSizeBytes} bytes, Duration: {Duration}ms",
                correlationId, result.TotalRecords, result.FileSizeBytes, stopwatch.ElapsedMilliseconds);

            await context.Publish(new ReportJobBatchProgressCommand
            {
                CorrelationId = correlationId,
                CompletedCount = 1,
                FailedCount = 0
            });

            await PublishCompletionEvent(context, message, stopwatch.Elapsed, result);
        }
        else
        {
            _logger.LogError(
                "Order report generation failed {CorrelationId}: {ErrorMessage}",
                correlationId, result.ErrorMessage);

            await PublishJobFailed(context, correlationId, result.ErrorMessage ?? "Report generation failed");
        }
    }

    private async Task PublishJobStarted(ConsumeContext<GenerateOrderReportCommand> context, GenerateOrderReportCommand message)
    {
        await context.Publish(new RequestJobCommand
        {
            JobType = nameof(JobType.ReportGeneration),
            CorrelationId = message.CorrelationId.ToString(),
            RequestedBy = message.RequestedBy,
            PayloadJson = JsonSerializer.Serialize(new
            {
                message.ReportType,
                message.Format,
                message.StatusFilter,
                message.BuyerIdFilter,
                message.SellerIdFilter,
                message.StartDate,
                message.EndDate
            }),
            TotalItems = 1,
            MaxRetryCount = 0
        });
    }

    private static async Task PublishCompletionEvent(
        ConsumeContext<GenerateOrderReportCommand> context,
        GenerateOrderReportCommand message,
        TimeSpan duration,
        OrderReportResult result)
    {
        await context.Publish(new OrderReportGeneratedEvent
        {
            CorrelationId = message.CorrelationId,
            RequestedBy = message.RequestedBy,
            ReportType = message.ReportType,
            Format = message.Format,
            TotalRecords = result.TotalRecords,
            FileName = result.FileName,
            ContentType = result.ContentType,
            FileSizeBytes = result.FileSizeBytes,
            DownloadUrl = string.Empty,
            Duration = duration,
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task PublishJobFailed(ConsumeContext<GenerateOrderReportCommand> context, string correlationId, string errorMessage)
    {
        await context.Publish(new FailJobByCorrelationCommand
        {
            CorrelationId = correlationId,
            ErrorMessage = errorMessage
        });
    }

    private static ReportType? ParseReportType(string value)
    {
        return Enum.TryParse<ReportType>(value, ignoreCase: true, out var result) ? result : null;
    }

    private static ReportFormat? ParseReportFormat(string value)
    {
        return Enum.TryParse<ReportFormat>(value, ignoreCase: true, out var result) ? result : null;
    }

    private static OrderStatus? ParseStatusFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null :
            Enum.TryParse<OrderStatus>(value, ignoreCase: true, out var result) ? result : null;
    }
}
