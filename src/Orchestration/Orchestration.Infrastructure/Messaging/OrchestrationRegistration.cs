using BuildingBlocks.Infrastructure.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchestration.Infrastructure.Persistence;
using Orchestration.Infrastructure.Scheduling;
using Orchestration.Sagas.AuctionCompletion;
using Orchestration.Sagas.BuyNow;

namespace Orchestration.Infrastructure.Messaging;

public static class OrchestrationRegistration
{
    public static IServiceCollection AddOrchestration(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<OrchestrationDbContext>(x => x.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<SagaDeadlineDispatcher>();
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                RabbitMqDeployment.ConfigureQuorumQueue(endpoint, context));
            x.SetKebabCaseEndpointNameFormatter();
            x.AddSagaStateMachine<BuyNowSagaStateMachine, BuyNowSagaState>().EntityFrameworkRepository(r =>
            {
                r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                r.ExistingDbContext<OrchestrationDbContext>();
                r.UsePostgres();
            });
            x.AddSagaStateMachine<AuctionCompletionSagaStateMachine, AuctionCompletionSagaState>().EntityFrameworkRepository(r =>
            {
                r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                r.ExistingDbContext<OrchestrationDbContext>();
                r.UsePostgres();
            });
            x.AddConsumer<SagaRecoveryConsumer>();
            x.AddEntityFrameworkOutbox<OrchestrationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromMilliseconds(200);
            });
            x.AddConfigureEndpointsCallback((context, _, e) =>
            {
                e.UseMessageRetry(r => r.Intervals(100, 300, 1000, 3000, 5000));
                e.UseEntityFrameworkOutbox<OrchestrationDbContext>(context);
            });
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureRabbitMqHost(config);
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddHostedService<SagaDeadlineWorker>();
        return services;
    }
}
