using System.Diagnostics;
using System.Threading.Channels;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Auctions.BulkImport;

public interface IBulkImportService
{
    Task<Guid> StartImportAsync(Stream fileStream, string fileName, Guid userId, string username);
    BulkImportProgress? GetProgress(Guid jobId);
    IEnumerable<BulkImportProgress> GetUserJobs(Guid userId);
    void CancelJob(Guid jobId);
}

public class BulkImportService : IBulkImportService
{
    private readonly IBulkImportJobStore _jobStore;
    private readonly Channel<BulkImportJob> _jobChannel;
    private readonly ILogger<BulkImportService> _logger;
    private readonly string _uploadPath;

    public BulkImportService(
        IBulkImportJobStore jobStore,
        Channel<BulkImportJob> jobChannel,
        ILogger<BulkImportService> logger)
    {
        _jobStore = jobStore;
        _jobChannel = jobChannel;
        _logger = logger;
        _uploadPath = Path.Combine(Path.GetTempPath(), "auction-bulk-imports");
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<Guid> StartImportAsync(Stream fileStream, string fileName, Guid userId, string username)
    {
        var filePath = Path.Combine(_uploadPath, $"{Guid.NewGuid()}_{fileName}");

        await using (var file = File.Create(filePath))
        {
            await fileStream.CopyToAsync(file);
        }

        var job = new BulkImportJob
        {
            UserId = userId,
            Username = username,
            FileName = fileName,
            FilePath = filePath,
            Status = BulkImportStatus.Pending
        };

        _jobStore.AddJob(job);
        await _jobChannel.Writer.WriteAsync(job);

        _logger.LogInformation("Bulk import job {JobId} queued for file {FileName}", job.Id, fileName);

        return job.Id;
    }

    public BulkImportProgress? GetProgress(Guid jobId)
    {
        var job = _jobStore.GetJob(jobId);
        if (job == null) return null;

        return MapToProgress(job);
    }

    public IEnumerable<BulkImportProgress> GetUserJobs(Guid userId)
    {
        return _jobStore.GetJobsByUser(userId).Select(MapToProgress);
    }

    public void CancelJob(Guid jobId)
    {
        var job = _jobStore.GetJob(jobId);
        if (job != null && job.Status is BulkImportStatus.Pending or BulkImportStatus.Processing)
        {
            job.Status = BulkImportStatus.Cancelled;
            job.CompletedAt = DateTimeOffset.UtcNow;
            _jobStore.UpdateJob(job);
        }
    }

    private static BulkImportProgress MapToProgress(BulkImportJob job)
    {
        var elapsed = job.StartedAt.HasValue
            ? (DateTimeOffset.UtcNow - job.StartedAt.Value).TotalSeconds
            : 0;

        var estimatedRemaining = 0;
        if (job.ProcessedRows > 0 && job.TotalRows > job.ProcessedRows)
        {
            var rate = job.ProcessedRows / elapsed;
            if (rate > 0)
            {
                estimatedRemaining = (int)((job.TotalRows - job.ProcessedRows) / rate);
            }
        }

        return new BulkImportProgress
        {
            JobId = job.Id,
            Status = job.Status,
            TotalRows = job.TotalRows,
            ProcessedRows = job.ProcessedRows,
            SuccessCount = job.SuccessCount,
            FailureCount = job.FailureCount,
            EstimatedSecondsRemaining = estimatedRemaining,
            RecentErrors = job.Errors.TakeLast(10).ToList(),
            ErrorMessage = job.ErrorMessage
        };
    }
}

public class BulkImportBackgroundService : BackgroundService
{
    private readonly Channel<BulkImportJob> _jobChannel;
    private readonly IBulkImportJobStore _jobStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IExcelService _excelService;
    private readonly ILogger<BulkImportBackgroundService> _logger;

    private const int BatchSize = 1000;
    private const int MaxErrors = 1000;

    public BulkImportBackgroundService(
        Channel<BulkImportJob> jobChannel,
        IBulkImportJobStore jobStore,
        IServiceScopeFactory scopeFactory,
        IExcelService excelService,
        ILogger<BulkImportBackgroundService> logger)
    {
        _jobChannel = jobChannel;
        _jobStore = jobStore;
        _scopeFactory = scopeFactory;
        _excelService = excelService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bulk import background service started");

        await foreach (var job in _jobChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk import job {JobId}", job.Id);
                job.Status = BulkImportStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTimeOffset.UtcNow;
                _jobStore.UpdateJob(job);
            }
            finally
            {
                CleanupJobFile(job);
            }
        }
    }

    private async Task ProcessJobAsync(BulkImportJob job, CancellationToken ct)
    {
        _logger.LogInformation("Processing bulk import job {JobId}", job.Id);

        job.Status = BulkImportStatus.Counting;
        job.StartedAt = DateTimeOffset.UtcNow;
        _jobStore.UpdateJob(job);

        var extension = Path.GetExtension(job.FileName).ToLowerInvariant();
        List<ImportAuctionDto> allRecords;

        await using (var stream = File.OpenRead(job.FilePath))
        {
            if (extension is ".xlsx" or ".xls")
            {
                allRecords = _excelService.ParseFile(stream, ParseExcelRow);
            }
            else if (extension == ".csv")
            {
                allRecords = await ParseCsvStreamingAsync(stream);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported file format: {extension}");
            }
        }

        job.TotalRows = allRecords.Count;
        job.Status = BulkImportStatus.Processing;
        _jobStore.UpdateJob(job);

        _logger.LogInformation("Job {JobId}: Found {TotalRows} records to import", job.Id, job.TotalRows);

        var stopwatch = Stopwatch.StartNew();
        var batches = allRecords.Chunk(BatchSize);

        foreach (var batch in batches)
        {
            if (job.Status == BulkImportStatus.Cancelled)
            {
                _logger.LogInformation("Job {JobId} was cancelled", job.Id);
                break;
            }

            await ProcessBatchAsync(job, batch.ToList(), ct);

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _jobStore.UpdateJob(job);
                stopwatch.Restart();
            }
        }

        job.Status = job.Status == BulkImportStatus.Cancelled
            ? BulkImportStatus.Cancelled
            : BulkImportStatus.Completed;
        job.CompletedAt = DateTimeOffset.UtcNow;
        _jobStore.UpdateJob(job);

        _logger.LogInformation(
            "Job {JobId} completed: {SuccessCount} succeeded, {FailureCount} failed",
            job.Id, job.SuccessCount, job.FailureCount);
    }

    private async Task ProcessBatchAsync(BulkImportJob job, List<ImportAuctionDto> batch, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var auctions = new List<Auction>();

        foreach (var dto in batch)
        {
            job.ProcessedRows++;

            try
            {
                var item = Item.Create(
                    dto.Title,
                    dto.Description,
                    dto.Condition,
                    dto.YearManufactured);

                var auction = Auction.Create(
                    job.UserId,
                    job.Username,
                    item,
                    dto.ReservePrice,
                    dto.AuctionEnd,
                    dto.Currency);

                auctions.Add(auction);
                job.SuccessCount++;
            }
            catch (Exception ex)
            {
                job.FailureCount++;
                if (job.Errors.Count < MaxErrors)
                {
                    job.Errors.Add(new BulkImportError
                    {
                        RowNumber = job.ProcessedRows,
                        Error = ex.Message
                    });
                }
            }
        }

        if (auctions.Count > 0)
        {
            await repository.AddRangeAsync(auctions);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private static ImportAuctionDto? ParseExcelRow(IExcelRowReader row)
    {
        if (row.IsEmpty) return null;

        return new ImportAuctionDto
        {
            Title = row.GetString("Title") ?? "",
            Description = row.GetString("Description") ?? "",
            Condition = row.GetString("Condition"),
            YearManufactured = row.GetInt("YearManufactured") is var year and > 0 ? year : null,
            ReservePrice = row.GetDecimal("ReservePrice"),
            Currency = row.GetString("Currency") ?? "USD",
            AuctionEnd = row.GetDateTimeOffset("AuctionEnd") ?? DateTimeOffset.UtcNow.AddDays(7)
        };
    }

    private static async Task<List<ImportAuctionDto>> ParseCsvStreamingAsync(Stream stream)
    {
        var records = new List<ImportAuctionDto>();
        using var reader = new StreamReader(stream);

        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(headerLine))
            return records;

        var headers = ParseCsvLine(headerLine);
        var headerIndex = headers
            .Select((h, i) => (Header: h.Trim(), Index: i))
            .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line);

            records.Add(new ImportAuctionDto
            {
                Title = GetValue(values, headerIndex, "Title") ?? "",
                Description = GetValue(values, headerIndex, "Description") ?? "",
                Condition = GetValue(values, headerIndex, "Condition"),
                YearManufactured = int.TryParse(GetValue(values, headerIndex, "YearManufactured"), out var year) ? year : null,
                ReservePrice = decimal.TryParse(GetValue(values, headerIndex, "ReservePrice"), out var price) ? price : 0,
                Currency = GetValue(values, headerIndex, "Currency") ?? "USD",
                AuctionEnd = DateTimeOffset.TryParse(GetValue(values, headerIndex, "AuctionEnd"), out var end)
                    ? end
                    : DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        return records;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (var c in line)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static string? GetValue(string[] values, Dictionary<string, int> index, string header)
    {
        if (!index.TryGetValue(header, out var i) || i >= values.Length) return null;
        var v = values[i].Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }

    private void CleanupJobFile(BulkImportJob job)
    {
        try
        {
            if (File.Exists(job.FilePath))
            {
                File.Delete(job.FilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup file for job {JobId}", job.Id);
        }
    }
}
