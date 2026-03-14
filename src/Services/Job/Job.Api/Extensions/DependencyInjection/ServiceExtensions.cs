using BuildingBlocks.Application.Abstractions;
using Jobs.Application.Interfaces;
using Jobs.Infrastructure.Messaging;
using Jobs.Infrastructure.Persistence;
using Jobs.Infrastructure.Persistence.Repositories;
using Jobs.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using System.Text.Json.Serialization;

namespace Jobs.Api.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName();
        });

        return builder;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<JobDbContext>(options =>
            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            }));

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobItemRepository, JobItemRepository>();
        services.AddScoped<IJobItemDispatcher, JobItemDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<JobProcessingWorker>();

        return services;
    }
}
