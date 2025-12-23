using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Common.Repository.Interfaces;
using Common.Repository.Implementations;
using Common.CQRS.Extensions;
using Serilog;
using System.Text.Json.Serialization;

namespace PaymentService.API.Extensions;

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

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var applicationAssembly = typeof(PaymentService.Application.DTOs.OrderDto).Assembly;
        services.AddCQRS(applicationAssembly);

        services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

        services.AddScoped<Application.Interfaces.IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();

        return services;
    }
}
