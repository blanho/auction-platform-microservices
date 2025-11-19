using Azure.Identity;
using Azure.Storage.Blobs;
using BuildingBlocks.Web.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Storage.Application.Configuration;
using Storage.Application.Interfaces;
using Storage.Application.Services;
using Storage.Infrastructure.Persistence;
using Storage.Infrastructure.Repositories;
using Storage.Infrastructure.Scanning;
using Storage.Infrastructure.Storage;

namespace Storage.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<StorageDbContext>(options =>
        {
            options.UseNpgsql(dataSource);
        });

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        services.AddScoped<global::Storage.Application.Interfaces.IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IFilePermissionService, FilePermissionService>();

        var storageType = configuration.GetValue<string>("Storage:DefaultProvider") ?? "azure";

        switch (storageType.ToLowerInvariant())
        {
            case "azure":
            case "blob":

                services.AddSingleton<BlobServiceClient>(sp =>
                {
                    var azureConnectionString = configuration["Storage:Azure:ConnectionString"];
                    var accountName = configuration["Storage:Azure:AccountName"];
                    var useManagedIdentity = configuration.GetValue<bool>("Storage:Azure:UseManagedIdentity");

                    if (!string.IsNullOrEmpty(azureConnectionString))
                    {

                        return new BlobServiceClient(azureConnectionString);
                    }

                    if (useManagedIdentity && !string.IsNullOrEmpty(accountName))
                    {

                        var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
                        return new BlobServiceClient(blobUri, new DefaultAzureCredential());
                    }

                    if (!string.IsNullOrEmpty(accountName))
                    {
                        var accountKey = configuration["Storage:Azure:AccountKey"];
                        if (!string.IsNullOrEmpty(accountKey))
                        {

                            var connStr = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
                            return new BlobServiceClient(connStr);
                        }
                    }

                    throw new ConfigurationException(
                        "Azure Blob Storage configuration is missing. " +
                        "Provide either ConnectionString, or AccountName with AccountKey/ManagedIdentity.");
                });

                services.AddSingleton<AzureBlobStorageProvider>();
                services.AddSingleton<IStorageProvider>(sp => sp.GetRequiredService<AzureBlobStorageProvider>());
                services.AddSingleton<IAzureBlobStorageProvider>(sp => sp.GetRequiredService<AzureBlobStorageProvider>());

                var azureScanningEnabled = configuration.GetValue<bool>("Storage:Scanning:Enabled");
                if (azureScanningEnabled)
                {
                    services.AddSingleton<IVirusScanService, AzureDefenderScanService>();
                }
                break;

            case "local":
            default:
                var basePath = configuration.GetValue<string>("Storage:LocalBasePath") ?? "uploads";
                services.AddSingleton<IStorageProvider>(sp =>
                {
                    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageOptions>>();
                    var config = sp.GetRequiredService<IConfiguration>();
                    return new LocalStorageProvider(options, config, basePath);
                });

                var localScanningEnabled = configuration.GetValue<bool>("Storage:Scanning:Enabled");
                if (localScanningEnabled)
                {
                    services.AddSingleton<IVirusScanService, LocalVirusScanService>();
                }
                break;
        }

        return services;
    }
}
