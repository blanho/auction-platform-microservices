using System.Text.Json;
using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Features.Auctions.ExportAuctions;
using Auctions.Application.Enums;
using Auctions.Application.Interfaces;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Services;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ExportAuctionsConsumer : IConsumer<ProcessAuctionExportCommand>
{
    private readonly IAuctionWorkflowStore _workflow;
    private readonly IAuctionReadRepository _readRepository;
    private readonly IEnumerable<IReportExporter> _exporters;
    private readonly AuctionExportStorageClient _storageClient;
    private readonly ILogger<ExportAuctionsConsumer> _logger;

    public ExportAuctionsConsumer(
        IAuctionWorkflowStore workflow,
        IAuctionReadRepository readRepository,
        IEnumerable<IReportExporter> exporters,
        AuctionExportStorageClient storageClient,
        ILogger<ExportAuctionsConsumer> logger)
    {
        _readRepository = readRepository;
        _exporters = exporters;
        _storageClient = storageClient;
        _logger = logger;
        _workflow = workflow;
    }

    public async Task Consume(ConsumeContext<ProcessAuctionExportCommand> context)
    {
        var message = context.Message;
        var receiptKey = $"ExportAuctionsConsumer:{message.CorrelationId}";
        if (await _workflow.ExistsAsync(receiptKey, context.CancellationToken))
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing auction export {CorrelationId} in {Format} format",
            correlationId, message.Format);

        await PublishJobRequestAsync(context, correlationId);

        var format = ParseExportFormat(message.Format);
        var exporter = _exporters.FirstOrDefault(candidate => candidate.Format == format);

        if (exporter is null)
        {
            _logger.LogWarning(
                "Unsupported export format {Format} for {CorrelationId}",
                message.Format, correlationId);

            await ReportUnsupportedFormatAsync(context, correlationId);

            await PublishCompletionEventAsync(context, message, new ExportCompletion { Duration = stopwatch.Elapsed });
            return;
        }

        var statusFilter = ParseStatusFilter(message.StatusFilter);
        var auctions = await _readRepository.GetAuctionsForExportAsync(
            statusFilter, message.SellerFilter, message.StartDate, message.EndDate,
            context.CancellationToken);

        var exportRows = ExportAuctionMapper.ToRows(auctions);
        var content = exporter.Export(exportRows);

        var fileName = $"auctions-export-{message.CorrelationId}{exporter.FileExtension}";

        stopwatch.Stop();

        var storedReport = await _storageClient.StoreReportAsync(
            content: content,
            fileName: fileName,
            contentType: exporter.ContentType,
            ownerId: message.RequestedBy,
            cancellationToken: context.CancellationToken,
            requestId: message.CorrelationId,
            recordCount: exportRows.Count);

        await PublishProgressAsync(context, correlationId);

        _logger.LogInformation(
            "Export {CorrelationId} completed: {RecordCount} auctions, {Size} bytes in {Duration}ms",
            correlationId, exportRows.Count, content.Length, stopwatch.ElapsedMilliseconds);

        await PublishCompletionEventAsync(context, message, new ExportCompletion
        {
            Duration = stopwatch.Elapsed,
            TotalRecords = storedReport.TotalRecords ?? exportRows.Count,
            FileName = storedReport.FileName ?? fileName,
            ContentType = storedReport.ContentType ?? exporter.ContentType,
            FileSizeBytes = storedReport.FileSizeBytes ?? content.Length,
            DownloadUrl = storedReport.DownloadUrl
        });
    }

    private static async Task PublishJobRequestAsync(
        ConsumeContext<ProcessAuctionExportCommand> context,
        string correlationId)
    {
        var message = context.Message;
        if (message.ParentJobId is not null)
        {
            return;
        }

        await context.Publish(new RequestJobCommand
        {
            JobType = nameof(JobType.DataExport),
            CorrelationId = correlationId,
            RequestedBy = message.RequestedBy,
            PayloadJson = JsonSerializer.Serialize(new
            {
                message.Format,
                message.StatusFilter,
                message.SellerFilter,
                message.StartDate,
                message.EndDate
            }),
            TotalItems = 1,
            MaxRetryCount = 0
        });
    }

    private static async Task ReportUnsupportedFormatAsync(
        ConsumeContext<ProcessAuctionExportCommand> context,
        string correlationId)
    {
        var message = context.Message;
        if (message.ParentJobId is not null)
        {
            return;
        }

        await context.Publish(new FailJobByCorrelationCommand
        {
            CorrelationId = correlationId,
            ErrorMessage = $"Unsupported export format: {message.Format}"
        });
    }

    private static async Task PublishProgressAsync(
        ConsumeContext<ProcessAuctionExportCommand> context,
        string correlationId)
    {
        var message = context.Message;
        if (message.ParentJobId is not null)
        {
            return;
        }

        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            BatchId = "export:complete",
            CompletedCount = 1,
            FailedCount = 0
        });
    }

    private static ExportFormat ParseExportFormat(string format)
    {
        return Enum.TryParse<ExportFormat>(format, ignoreCase: true, out var parsed)
            ? parsed
            : ExportFormat.Csv;
    }

    private static Status? ParseStatusFilter(string? statusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter))
        {
            return null;
        }

        return Enum.TryParse<Status>(statusFilter, ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }

    private async Task PublishCompletionEventAsync(
        ConsumeContext<ProcessAuctionExportCommand> context,
        ProcessAuctionExportCommand message,
        ExportCompletion completion)
    {
        await _workflow.CompleteAsync($"ExportAuctionsConsumer:{message.CorrelationId}", message.CorrelationId,
            completion.TotalRecords, 0, [], 1, context.CancellationToken);
        if (message.ParentJobId.HasValue && message.ParentJobItemId.HasValue)
        {
            var hasDownload = !string.IsNullOrEmpty(completion.DownloadUrl);
            await context.Publish(new ReportJobItemResultCommand
            {
                JobId = message.ParentJobId.Value,
                JobItemId = message.ParentJobItemId.Value,
                Attempt = message.Attempt,
                IsSuccess = hasDownload,
                IsFinalFailure = true,
                ErrorMessage = hasDownload ? null : "Auction workflow completed with errors."
            });
        }

        await context.Publish(new AuctionExportCompletedEvent
        {
            CorrelationId = message.CorrelationId,
            RequestedBy = message.RequestedBy,
            Format = message.Format,
            TotalRecords = completion.TotalRecords,
            FileName = completion.FileName,
            ContentType = completion.ContentType,
            FileSizeBytes = completion.FileSizeBytes,
            DownloadUrl = completion.DownloadUrl,
            Duration = completion.Duration,
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    private sealed record ExportCompletion
    {
        public TimeSpan Duration { get; init; }
        public int TotalRecords { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public long FileSizeBytes { get; init; }
        public string DownloadUrl { get; init; } = string.Empty;
    }
}
