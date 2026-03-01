using Jobs.Api.Extensions.DependencyInjection;
using Jobs.Infrastructure.Extensions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using BuildingBlocks.Web.OpenApi;
using BuildingBlocks.Application.Extensions;
using Carter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "JobService",
    requiresDatabase: true,
    requiresRedis: false,
    requiresRabbitMQ: true,
    requiresIdentity: true);

builder.AddApplicationLogging();

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);
builder.Services.AddCQRS(typeof(Jobs.Application.Features.Jobs.CreateJob.CreateJobCommand).Assembly);
builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCarter();
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddCustomHealthChecks(
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "JobService");

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
app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.UseCommonOpenApi();
    app.UseCommonSwaggerUI("Job Service");
}

app.Run();

public partial class Program { }
