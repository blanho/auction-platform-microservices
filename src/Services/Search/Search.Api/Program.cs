using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using Carter;
using Search.Api.Extensions.DependencyInjection;
using Search.Api.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "SearchService",
    requiresDatabase: false,
    requiresRedis: true,
    requiresRabbitMQ: true,
    requiresIdentity: false);

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCarter();
builder.Services.AddSearchServices(builder.Configuration);
builder.Services.AddSearchMessaging(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddAppLocalization<SearchResources>();
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAuthorization();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
    elasticsearchUri: builder.Configuration["Elasticsearch:Uri"],
    serviceName: "SearchService");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API v1"));
}

app.UseCorrelationId();
app.UseRequestTracing();
app.UseAppExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();
app.MapCustomHealthChecks();
app.MapCarter();

await app.RunAsync();

public partial class Program { }
