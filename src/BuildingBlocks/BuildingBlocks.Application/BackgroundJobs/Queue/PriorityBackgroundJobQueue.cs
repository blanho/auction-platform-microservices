using System.Threading.Channels;
using BuildingBlocks.Application.BackgroundJobs.Core;

namespace BuildingBlocks.Application.BackgroundJobs.Queue;

public sealed class PriorityBackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<BackgroundJobState> _criticalChannel;
    private readonly Channel<BackgroundJobState> _highChannel;
    private readonly Channel<BackgroundJobState> _normalChannel;
    private readonly Channel<BackgroundJobState> _lowChannel;

    private int _pendingCount;
    public int PendingCount => _pendingCount;

    public PriorityBackgroundJobQueue(BackgroundJobQueueOptions? options = null)
    {
        options ??= new BackgroundJobQueueOptions();

        var criticalOptions = CreateChannelOptions(options.CriticalCapacity);
        var highOptions = CreateChannelOptions(options.HighCapacity);
        var normalOptions = CreateChannelOptions(options.NormalCapacity);
        var lowOptions = CreateChannelOptions(options.LowCapacity);

        _criticalChannel = Channel.CreateBounded<BackgroundJobState>(criticalOptions);
        _highChannel = Channel.CreateBounded<BackgroundJobState>(highOptions);
        _normalChannel = Channel.CreateBounded<BackgroundJobState>(normalOptions);
        _lowChannel = Channel.CreateBounded<BackgroundJobState>(lowOptions);
    }

    public async ValueTask EnqueueAsync(BackgroundJobState job, CancellationToken ct = default)
    {
        job.MarkQueued();

        var channel = GetChannelForPriority(job.Priority);
        await channel.Writer.WriteAsync(job, ct);

        Interlocked.Increment(ref _pendingCount);
    }

    public async ValueTask<BackgroundJobState> DequeueAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (TryReadFromChannel(_criticalChannel, out var job))
            {
                Interlocked.Decrement(ref _pendingCount);
                return job!;
            }

            if (TryReadFromChannel(_highChannel, out job))
            {
                Interlocked.Decrement(ref _pendingCount);
                return job!;
            }

            if (TryReadFromChannel(_normalChannel, out job))
            {
                Interlocked.Decrement(ref _pendingCount);
                return job!;
            }

            if (TryReadFromChannel(_lowChannel, out job))
            {
                Interlocked.Decrement(ref _pendingCount);
                return job!;
            }

            await WaitForAnyChannelAsync(ct);
        }

        throw new OperationCanceledException(ct);
    }

    private Channel<BackgroundJobState> GetChannelForPriority(BackgroundJobPriority priority)
    {
        return priority switch
        {
            BackgroundJobPriority.Critical => _criticalChannel,
            BackgroundJobPriority.High => _highChannel,
            BackgroundJobPriority.Low => _lowChannel,
            _ => _normalChannel
        };
    }

    private static bool TryReadFromChannel(Channel<BackgroundJobState> channel, out BackgroundJobState? job)
    {
        return channel.Reader.TryRead(out job);
    }

    private async Task WaitForAnyChannelAsync(CancellationToken ct)
    {
        var criticalTask = _criticalChannel.Reader.WaitToReadAsync(ct).AsTask();
        var highTask = _highChannel.Reader.WaitToReadAsync(ct).AsTask();
        var normalTask = _normalChannel.Reader.WaitToReadAsync(ct).AsTask();
        var lowTask = _lowChannel.Reader.WaitToReadAsync(ct).AsTask();

        await Task.WhenAny(criticalTask, highTask, normalTask, lowTask);
    }

    private static BoundedChannelOptions CreateChannelOptions(int capacity)
    {
        return new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
    }
}

public sealed class BackgroundJobQueueOptions
{
    public int CriticalCapacity { get; set; } = 10;
    public int HighCapacity { get; set; } = 50;
    public int NormalCapacity { get; set; } = 100;
    public int LowCapacity { get; set; } = 200;
}
