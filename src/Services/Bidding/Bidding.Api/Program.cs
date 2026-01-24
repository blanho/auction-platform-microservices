using Bidding.Api.Extensions;
using Bidding.Infrastructure.Extensions;
using Bidding.Infrastructure.Persistence;
using Bidding.Infrastructure.Grpc;
using Bidding.Application.Interfaces;
using Auctions.Api.Grpc;
using Carter;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.OpenApi;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

builder.Services.AddCommonUtilities();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

builder.Services.AddCarter();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddGrpcClient<AuctionGrpc.AuctionGrpcClient>(options =>
{
    var auctionGrpcUrl = builder.Configuration["GrpcServices:AuctionService"]
        ?? "https://localhost:7001";
    options.Address = new Uri(auctionGrpcUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddScoped<IAuctionGrpcClient, AuctionGrpcClient>();

var identityAuthority = builder.Configuration["Identity:Authority"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = identityAuthority;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = identityAuthority,
        ValidateAudience = true,
        ValidAudience = "auctionApp",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();

builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: builder.Configuration.GetConnectionString("RabbitMQ"),
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "BiddingService");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BidDbContext>();
    db.Database.Migrate();
}

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseAppExceptionHandling();

app.MapCustomHealthChecks();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAccessAuthorization();

app.MapCarter();

app.MapGrpcReflectionService();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Bidding Service");

app.Run();