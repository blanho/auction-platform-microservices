using Payment.Application.Interfaces;
using Payment.Infrastructure.Configuration;
using Payment.Infrastructure.Extensions;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;
using Payment.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Application.Extensions;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Payment.Api.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();
        });

        return builder;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
            });

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        });

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(30);
                }));

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var applicationAssembly = typeof(Payment.Application.DTOs.OrderDto).Assembly;
        services.AddCQRS(applicationAssembly);

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

        // Register UnitOfWork - required by command handlers, consumers, and StripePaymentService
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());

        // Register distributed locking - required by wallet operations (HoldFunds, ReleaseFunds, Withdraw, ProcessWalletPayment)
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddDistributedLocking(redisConnectionString);

        services.AddOptions<StripeOptions>()
            .BindConfiguration(StripeOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IStripeServiceFactory, StripeServiceFactory>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        services.AddScoped<IStripePaymentService, StripePaymentService>();

        return services;
    }
}
