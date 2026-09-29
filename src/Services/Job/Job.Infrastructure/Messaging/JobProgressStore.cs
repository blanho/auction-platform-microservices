using Jobs.Application.Interfaces;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Jobs.Infrastructure.Persistence;

namespace Jobs.Infrastructure.Messaging;

public class JobProgressStore(JobDbContext db, IUnitOfWork unitOfWork)
{
    public async Task LockAsync(string correlationId, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is not null)
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({correlationId}, 0))", ct);
    }

    public async Task RecordAsync(JobProgressEntry entry, CancellationToken ct)
    {
        await LockAsync(entry.CorrelationId, ct);
        if (entry.CompletedCount < 0 || entry.FailedCount < 0 || string.IsNullOrWhiteSpace(entry.BatchId))
            throw new ArgumentException("A batch identity and nonnegative counts are required.");
        if (await db.ProgressEntries.AnyAsync(x => x.CorrelationId == entry.CorrelationId && x.BatchId == entry.BatchId, ct))
            return;
        db.ProgressEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        var job = await db.Jobs.SingleOrDefaultAsync(x => x.CorrelationId == entry.CorrelationId, ct);
        if (job is not null) await ApplyAsync(job, ct);
    }

    public async Task ApplyAsync(Job job, CancellationToken ct)
    {
        var entries = await db.ProgressEntries
            .Where(x => x.CorrelationId == job.CorrelationId && !x.Applied).ToListAsync(ct);
        if (entries.Count == 0) return;
        if (job.Status is JobStatus.Pending or JobStatus.Processing)
        {
            var failure = entries.FirstOrDefault(x => x.ErrorMessage != null);
            if (failure is not null) job.Fail(failure.ErrorMessage!);
            else job.RecordBatchProgress(entries.Sum(x => x.CompletedCount), entries.Sum(x => x.FailedCount));
        }
        foreach (var entry in entries) entry.Applied = true;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
