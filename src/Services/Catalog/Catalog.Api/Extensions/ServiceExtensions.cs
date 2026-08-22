using Microsoft.EntityFrameworkCore.Diagnostics;
using Catalog.Application.Mappings;
using Catalog.Api.Extensions;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using BuildingBlocks.Infrastructure.Repository;
using AutoMapper;
using Npgsql;
using System.Text.Json.Serialization;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Infrastructure.Caching;

namespace Catalog.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCatalogServices(this IServiceCollection services, IConfiguration configuration)
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

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            })
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddAutoMapper(_ => { }, typeof(CatalogMappingProfile).Assembly);

        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, CatalogUnitOfWork>();

        return services;
    }
}
