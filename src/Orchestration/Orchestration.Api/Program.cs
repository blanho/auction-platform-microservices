using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using Microsoft.EntityFrameworkCore;
using Orchestration.Api.Endpoints;
using Orchestration.Infrastructure.Messaging;
using Orchestration.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ValidateStandardConfiguration(builder.Configuration, "OrchestrationService",
    requiresDatabase: true, requiresRedis: false, requiresRabbitMQ: true, requiresIdentity: true);
builder.Services.AddOrchestration(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment, options => options.MapInboundClaims = false);
builder.Services.AddAuthorization();
builder.Services.AddCustomHealthChecks(databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "OrchestrationService");
var app = builder.Build();
var migrateOnly = args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase);
if (migrateOnly || builder.Configuration.GetValue("Database:AutoMigrate", !app.Environment.IsProduction()))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<OrchestrationDbContext>().Database.MigrateAsync();
}
if (migrateOnly)
{
    return;
}
app.UseAuthentication();
app.UseAuthorization();
app.MapCustomHealthChecks();
app.MapSagaRecovery();
await app.RunAsync();
public partial class Program { }
