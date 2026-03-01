using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ExportAuctionsConsumer : IConsumer<ProcessAuctionExportCommand>
{
    private readonly IAuctionExportRepository _exportRepository;
    private readonly IEnumerable<IReportExporter> _exporters;
    private readonly ILogger<ExportAuctionsConsumer> _logger;

    public ExportAuctionsConsumer(
        IAuctionExportRepository exportRepository,
        IEnumerable<IReportExporter> exporters,
        ILogger<ExportAuctionsConsumer> logger)
    {
        _exportRepository = exportRepository;
        _exporters = exporters;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessAuctionExportCommand> context)
    {
        var message = context.Message;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing auction export {CorrelationId} in {Format} format",
            correlationId, message.Format);

        await context.Publish(new RequestJobCommand
        {
            JobType = nameof(JobType.DataExport),
            CorrelationId = correlationId,
            RequestedBy = message.RequestedBy,
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(new
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
        var auctions = await _exportRepository.GetAuctionsForExportAsync(
            statusFilter, message.SellerFilter, message.StartDate, message.EndDate,
            context.CancellationToken);

        var exportRows = MapToExportRows(auctions);
        var content = exporter.Export(exportRows);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"auctions-export-{timestamp}{exporter.FileExtension}";

        stopwatch.Stop();

        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            CompletedCount = 1,
            FailedCount = 0
        });

        _logger.LogInformation(
            "Export {CorrelationId} completed: {RecordCount} auctions, {Size} bytes in {Duration}ms",
            correlationId, exportRows.Count, content.Length, stopwatch.ElapsedMilliseconds);

        await PublishCompletionEvent(context, message, stopwatch.Elapsed,
            exportRows.Count, fileName, exporter.ContentType, content.Length, downloadUrl: string.Empty);
    }

    private static ExportFormat ParseExportFormat(string format)
    {
        return Enum.TryParse<ExportFormat>(format, ignoreCase: true, out var parsed)
            ? parsed
            : ExportFormat.Csv;
    }

    private static Auctions.Domain.Enums.Status? ParseStatusFilter(string? statusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter))
        {
            return null;
        }

        return Enum.TryParse<Auctions.Domain.Enums.Status>(statusFilter, ignoreCase: true, out var parsed)
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
            Category: a.Item.Category?.Name,
            Condition: a.Item.Condition)).ToList();
    }

    private static async Task PublishCompletionEvent(
        ConsumeContext<ProcessAuctionExportCommand> context,
        ProcessAuctionExportCommand message,
        TimeSpan duration,
        int totalRecords,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string downloadUrl)
    {
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
