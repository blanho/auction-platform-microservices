using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Storage;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Storage.Infrastructure.Extensions;
using Storage.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "StorageService",
    requiresDatabase: true,
    requiresRedis: false,
    requiresRabbitMQ: true,
    requiresIdentity: true);

var applicationAssembly = typeof(Storage.Application.DTOs.StoredFileDto).Assembly;

builder.Services.AddCommonUtilities();
builder.Services.AddObservability(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(applicationAssembly);
builder.Services.AddCQRS(typeof(Storage.Application.Features.Files.UploadFile.UploadFileCommand).Assembly);
builder.Services.AddCarter();
builder.Services.AddFileStorage(builder.Configuration);
builder.Services.AddDbContext<StorageDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        }));
builder.Services.AddStorageInfrastructure();
builder.Services.AddStorageMessaging(builder.Configuration);
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddCustomHealthChecks(
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "StorageService");
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseApiSecurityHeaders();
app.UseCorrelationId();
app.UseRequestTracing();
app.UseAppExceptionHandling();
app.MapCustomHealthChecks();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var storageSettings = builder.Configuration.GetSection(FileStorageSettings.SectionName).Get<FileStorageSettings>();
if (storageSettings?.Provider == "Local")
{
    var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, storageSettings.Local.BasePath);
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
        RequestPath = storageSettings.Local.BaseUrl
    });
}

app.MapCarter();
app.MapControllers();

app.Run();
