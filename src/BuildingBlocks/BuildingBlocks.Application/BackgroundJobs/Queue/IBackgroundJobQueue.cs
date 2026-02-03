using System.Threading.Channels;
using BuildingBlocks.Application.BackgroundJobs.Core;

namespace BuildingBlocks.Application.BackgroundJobs.Queue;

public interface IBackgroundJobQueue
{
    ValueTask EnqueueAsync(BackgroundJobState job, CancellationToken ct = default);
    ValueTask<BackgroundJobState> DequeueAsync(CancellationToken ct);
    int PendingCount { get; }
}
