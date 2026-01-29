using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Observability;
using Carter;
using Search.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddObservability(builder.Configuration);

builder.Services.AddCarter();

builder.Services.AddSearchServices(builder.Configuration);

builder.Services.AddSearchMessaging(builder.Configuration);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Search Service API",
        Version = "v1",
        Description = "Auction Platform Search Service - Elasticsearch-backed search for auctions"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API v1");
    });
}

app.UseRequestTracing();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.UseAccessAuthorization();

app.MapHealthChecks("/health");

app.MapCarter();

app.Run();

public partial class Program { }
