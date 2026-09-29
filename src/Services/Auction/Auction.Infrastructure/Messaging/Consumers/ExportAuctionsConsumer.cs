using System.Text.Json;
using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
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
        if (await _workflow.ExistsAsync(receiptKey, context.CancellationToken)) return;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing auction export {CorrelationId} in {Format} format",
            correlationId, message.Format);

        if (message.ParentJobId is null)
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

        var format = ParseExportFormat(message.Format);
        var exporter = ResolveExporter(format);

        if (exporter is null)
        {
            _logger.LogWarning(
                "Unsupported export format {Format} for {CorrelationId}",
                message.Format, correlationId);

            if (message.ParentJobId is null)
                await context.Publish(new FailJobByCorrelationCommand
                {
                    CorrelationId = correlationId,
                    ErrorMessage = $"Unsupported export format: {message.Format}"
                });

            await PublishCompletionEvent(context, message, stopwatch.Elapsed, 0,
                fileName: string.Empty, contentType: string.Empty, fileSizeBytes: 0, downloadUrl: string.Empty);
            return;
        }

        var statusFilter = ParseStatusFilter(message.StatusFilter);
        var auctions = await _readRepository.GetAuctionsForExportAsync(
            statusFilter, message.SellerFilter, message.StartDate, message.EndDate,
            context.CancellationToken);

        var exportRows = MapToExportRows(auctions);
        var content = exporter.Export(exportRows);

        var fileName = $"auctions-export-{message.CorrelationId}{exporter.FileExtension}";

        stopwatch.Stop();

        var storedReport = await _storageClient.StoreReportAsync(
            content, fileName, exporter.ContentType, message.RequestedBy, context.CancellationToken, message.CorrelationId, exportRows.Count);

        if (message.ParentJobId is null)
            await context.Publish(new ReportJobBatchProgressCommand
            {
                CorrelationId = correlationId,
                BatchId = "export:complete",
                CompletedCount = 1,
                FailedCount = 0
            });

        _logger.LogInformation(
            "Export {CorrelationId} completed: {RecordCount} auctions, {Size} bytes in {Duration}ms",
            correlationId, exportRows.Count, content.Length, stopwatch.ElapsedMilliseconds);

        await PublishCompletionEvent(context, message, stopwatch.Elapsed,
            storedReport.TotalRecords ?? exportRows.Count, storedReport.FileName ?? fileName,
            storedReport.ContentType ?? exporter.ContentType, storedReport.FileSizeBytes ?? content.Length, storedReport.DownloadUrl);
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

    private IReportExporter? ResolveExporter(ExportFormat format)
    {
        return _exporters.FirstOrDefault(e => e.Format == format);
    }

    private static List<ExportAuctionRow> MapToExportRows(List<Auction> auctions)
    {
        return auctions.Select(a => new ExportAuctionRow(
            AuctionId: a.Id,
            Title: a.Item.Title,
            Seller: a.SellerUsername,
            Status: a.Status.ToString(),
            Currency: a.Currency,
            ReservePrice: a.ReservePrice,
            CurrentHighBid: a.CurrentHighBid,
            SoldAmount: a.SoldAmount,
            CreatedAt: a.CreatedAt,
            AuctionEnd: a.AuctionEnd,
            Category: a.Item.CategoryName,
            Condition: a.Item.Condition)).ToList();
    }

    private async Task PublishCompletionEvent(
        ConsumeContext<ProcessAuctionExportCommand> context,
        ProcessAuctionExportCommand message,
        TimeSpan duration,
        int totalRecords,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string downloadUrl)
    {
        await _workflow.CompleteAsync($"ExportAuctionsConsumer:{message.CorrelationId}", message.CorrelationId,
            totalRecords, 0, [], 1, context.CancellationToken);
        if (message.ParentJobId.HasValue && message.ParentJobItemId.HasValue)
            await context.Publish(new ReportJobItemResultCommand
            {
                JobId = message.ParentJobId.Value,
                JobItemId = message.ParentJobItemId.Value,
                Attempt = message.Attempt,
                IsSuccess = !string.IsNullOrEmpty(downloadUrl),
                IsFinalFailure = true,
                ErrorMessage = !string.IsNullOrEmpty(downloadUrl) ? null : "Auction workflow completed with errors."
            });
        await context.Publish(new AuctionExportCompletedEvent
        {
            CorrelationId = message.CorrelationId,
            RequestedBy = message.RequestedBy,
            Format = message.Format,
            TotalRecords = totalRecords,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            DownloadUrl = downloadUrl,
            Duration = duration,
            CompletedAt = DateTimeOffset.UtcNow
        });
    }
}
