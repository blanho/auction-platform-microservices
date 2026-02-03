using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Application.BackgroundJobs.Core;
using BuildingBlocks.Application.BackgroundJobs.Processing;
using BuildingBlocks.Application.BackgroundJobs.Queue;
using BuildingBlocks.Application.BackgroundJobs.Services;
using BuildingBlocks.Application.BackgroundJobs.Storage;

namespace BuildingBlocks.Application.BackgroundJobs;

public static class BackgroundJobsServiceExtensions
{
    public static BackgroundJobsBuilder AddBackgroundJobs(this IServiceCollection services)
    {
        return new BackgroundJobsBuilder(services);
    }
}

public sealed class BackgroundJobsBuilder
{
    private readonly IServiceCollection _services;
    private BackgroundJobQueueOptions _queueOptions = new();
    private BackgroundJobProcessorOptions _processorOptions = new();
    private bool _useInMemoryStore = true;

    public BackgroundJobsBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public BackgroundJobsBuilder WithQueueOptions(Action<BackgroundJobQueueOptions> configure)
    {
        configure(_queueOptions);
        return this;
    }

    public BackgroundJobsBuilder WithProcessorOptions(Action<BackgroundJobProcessorOptions> configure)
    {
        configure(_processorOptions);
        return this;
    }

    public BackgroundJobsBuilder WithConcurrentWorkers(int count)
    {
        _processorOptions.MaxConcurrentJobs = count;
        return this;
    }

    public BackgroundJobsBuilder WithJobTimeout(TimeSpan timeout)
    {
        _processorOptions.JobTimeout = timeout;
        return this;
    }

    public BackgroundJobsBuilder WithStore<TStore>() where TStore : class, IBackgroundJobStore
    {
        _useInMemoryStore = false;
        _services.AddSingleton<IBackgroundJobStore, TStore>();
        return this;
    }

    public BackgroundJobsBuilder AddHandler<THandler>() where THandler : class, IBackgroundJobHandler
    {
        _services.AddScoped<IBackgroundJobHandler, THandler>();
        return this;
    }

    public IServiceCollection Build()
    {
        _services.AddSingleton(_queueOptions);
        _services.AddSingleton(_processorOptions);

        _services.AddSingleton<IBackgroundJobQueue, PriorityBackgroundJobQueue>();
        _services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        if (_useInMemoryStore)
        {
            _services.AddSingleton<IBackgroundJobStore, InMemoryBackgroundJobStore>();
        }

        _services.AddHostedService<BackgroundJobProcessor>();

        return _services;
    }
}
