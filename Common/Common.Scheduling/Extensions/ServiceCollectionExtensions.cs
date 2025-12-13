using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Common.Scheduling.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScheduling(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollectionQuartzConfigurator>? configureJobs = null)
    {
        services.AddQuartz(q =>
        {
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });
            
            configureJobs?.Invoke(q);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
            options.AwaitApplicationStarted = true;
        });

        return services;
    }

    public static IServiceCollectionQuartzConfigurator AddCronJob<TJob>(
        this IServiceCollectionQuartzConfigurator q,
        string cronExpression,
        string? jobId = null,
        string? description = null,
        bool runOnStartup = false) where TJob : class, IJob
    {
        var jobKey = new JobKey(jobId ?? typeof(TJob).Name);

        q.AddJob<TJob>(opts => opts
            .WithIdentity(jobKey)
            .WithDescription(description ?? typeof(TJob).Name)
            .StoreDurably());

        q.AddTrigger(opts =>
        {
            var trigger = opts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}-trigger")
                .WithCronSchedule(cronExpression, x => x
                    .WithMisfireHandlingInstructionFireAndProceed());
                
            if (runOnStartup)
            {
                trigger.StartNow();
            }
        });

        return q;
    }

    public static IServiceCollectionQuartzConfigurator AddIntervalJob<TJob>(
        this IServiceCollectionQuartzConfigurator q,
        TimeSpan interval,
        string? jobId = null,
        string? description = null,
        bool runOnStartup = false) where TJob : class, IJob
    {
        var jobKey = new JobKey(jobId ?? typeof(TJob).Name);

        q.AddJob<TJob>(opts => opts
            .WithIdentity(jobKey)
            .WithDescription(description ?? typeof(TJob).Name)
            .StoreDurably());

        q.AddTrigger(opts =>
        {
            var trigger = opts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}-trigger")
                .WithSimpleSchedule(x => x
                    .WithInterval(interval)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionFireNow());
                    
            if (runOnStartup)
            {
                trigger.StartNow();
            }
            else
            {
                trigger.StartAt(DateTimeOffset.UtcNow.Add(interval));
            }
        });

        return q;
    }
}
