using MassTransit;
using System.Diagnostics;
using System.Text.Json;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
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
    private readonly IReportStorageClient _reportStorageClient;
    private readonly ILogger<GenerateOrderReportConsumer> _logger;

    public GenerateOrderReportConsumer(
        IOrderReportGenerator reportGenerator,
        IReportStorageClient reportStorageClient,
        ILogger<GenerateOrderReportConsumer> logger)
    {
        _reportGenerator = reportGenerator;
        _reportStorageClient = reportStorageClient;
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

        if (!message.ParentJobId.HasValue)
            await PublishJobStartedAsync(context, message);

        var report = ParseReportRequest(message);
        if (report is null)
        {
            _logger.LogWarning(
                "Invalid report type or format for {CorrelationId}: Type={ReportType}, Format={Format}",
                correlationId, message.ReportType, message.Format);

            await PublishJobFailedAsync(context, "Invalid report type, format, status filter, or date range.");
            return;
        }

        var result = await _reportGenerator.GenerateReportAsync(
            report.Type, report.Format, report.Parameters, context.CancellationToken);

        stopwatch.Stop();

        if (!result.Success)
        {
            _logger.LogError(
                "Order report generation failed {CorrelationId}: {ErrorMessage}",
                correlationId, result.ErrorMessage);

            await PublishJobFailedAsync(context, result.ErrorMessage ?? "Report generation failed");
            return;
        }

        var downloadUrl = await _reportStorageClient.StoreAsync(
            result, message.RequestedBy, context.CancellationToken);

        _logger.LogInformation(
            "Order report generation completed {CorrelationId} - Records: {TotalRecords}, Size: {FileSizeBytes} bytes, Duration: {Duration}ms",
            correlationId, result.TotalRecords, result.FileSizeBytes, stopwatch.ElapsedMilliseconds);

        if (message.ParentJobId.HasValue && message.ParentJobItemId.HasValue)
            await context.Publish(new ReportJobItemResultCommand
            {
                JobId = message.ParentJobId.Value,
                JobItemId = message.ParentJobItemId.Value,
                Attempt = message.Attempt,
                IsSuccess = true
            });
        else
            await context.Publish(new ReportJobBatchProgressCommand
            {
                CorrelationId = correlationId,
                BatchId = "report:complete",
                CompletedCount = 1,
                FailedCount = 0
            });

        await PublishCompletionEventAsync(context, message, stopwatch.Elapsed, result, downloadUrl);
    }

    private async Task PublishJobStartedAsync(ConsumeContext<GenerateOrderReportCommand> context, GenerateOrderReportCommand message)
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

    private static async Task PublishCompletionEventAsync(
        ConsumeContext<GenerateOrderReportCommand> context,
        GenerateOrderReportCommand message,
        TimeSpan duration,
        OrderReportResult result,
        string downloadUrl)
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
            DownloadUrl = downloadUrl,
            Duration = duration,
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task PublishJobFailedAsync(ConsumeContext<GenerateOrderReportCommand> context, string errorMessage)
    {
        var message = context.Message;
        if (message.ParentJobId.HasValue && message.ParentJobItemId.HasValue)
            await context.Publish(new ReportJobItemResultCommand
            {
                JobId = message.ParentJobId.Value,
                JobItemId = message.ParentJobItemId.Value,
                Attempt = message.Attempt,
                IsSuccess = false,
                IsFinalFailure = true,
                ErrorMessage = errorMessage
            });
        else
            await context.Publish(new FailJobByCorrelationCommand
            {
                CorrelationId = message.CorrelationId.ToString(),
                ErrorMessage = errorMessage
            });
    }

    private static ReportRequest? ParseReportRequest(GenerateOrderReportCommand message)
    {
        var type = ParseDefinedEnum<ReportType>(message.ReportType);
        var format = ParseDefinedEnum<ReportFormat>(message.Format);
        var status = ParseDefinedEnum<OrderStatus>(message.StatusFilter);
        if (type is null || format is null
            || (!string.IsNullOrWhiteSpace(message.StatusFilter) && status is null)
            || (message.StartDate.HasValue && message.EndDate.HasValue && message.EndDate <= message.StartDate))
        {
            return null;
        }

        var parameters = new OrderReportParameters(
            StatusFilter: status,
            BuyerIdFilter: message.BuyerIdFilter,
            SellerIdFilter: message.SellerIdFilter,
            StartDate: message.StartDate,
            EndDate: message.EndDate,
            Culture: message.Culture ?? "en-US");
        return new ReportRequest(type.Value, format.Value, parameters);
    }

    private static T? ParseDefinedEnum<T>(string? value) where T : struct, Enum =>
        Enum.TryParse<T>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed) ? parsed : null;

    private sealed record ReportRequest(ReportType Type, ReportFormat Format, OrderReportParameters Parameters);
}
