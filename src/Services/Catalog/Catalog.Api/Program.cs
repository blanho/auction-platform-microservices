using Catalog.Api.Extensions;
using Catalog.Infrastructure.Persistence;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Web.Observability;
using Carter;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "CatalogService",
    requiresDatabase: true,
    requiresRedis: false,
    requiresRabbitMQ: true,
    requiresIdentity: true);

builder.AddCentralizedLogging();

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddSanitization();
builder.Services.AddCatalogServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);
builder.Services.AddCQRS(typeof(Catalog.Application.Features.Brands.GetBrands.GetBrandsQuery).Assembly);
builder.Services.AddAuditServices(builder.Configuration, "catalog-service");
builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCarter();
builder.Services.AddGrpc();
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddCustomHealthChecks(
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "CatalogService");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
}

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrWhiteSpace(pathBase))
    app.UsePathBase(pathBase);

app.UseApiSecurityHeaders();
app.UseCorrelationIdLogging();
app.UseCorrelationId();
app.UseRequestTracing();
app.UseSerilogRequestLogging();
app.UseAppExceptionHandling();
app.MapCustomHealthChecks();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.MapGrpcService<Catalog.Api.Grpc.CatalogGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.UseCommonOpenApi();
    app.UseCommonSwaggerUI("Catalog Service");
}

await app.RunAsync();

public static partial class Program { }
