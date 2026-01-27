using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using BuildingBlocks.Application.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Analytics.Api.Consumers;
using Analytics.Api.Data;
using Analytics.Api.Extensions;
using Analytics.Api.Grpc;
using Analytics.Api.Interfaces;
using Analytics.Api.Middleware;
using Analytics.Api.Repositories;
using Analytics.Api.Services;
using BidService.Contracts.Grpc;
using IdentityService.Contracts.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5005, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5006, o => o.Protocols = HttpProtocols.Http2);
});

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "AnalyticsService:";
});

var auctionServiceGrpcUrl = builder.Configuration["GrpcServices:AuctionService"] ?? "http://localhost:7002";
builder.Services.AddGrpcClient<AuctionGrpc.AuctionGrpcClient>(options =>
{
    options.Address = new Uri(auctionServiceGrpcUrl);
});

var bidServiceGrpcUrl = builder.Configuration["GrpcServices:BidService"] ?? "http://localhost:7004";
builder.Services.AddGrpcClient<BidStatsGrpc.BidStatsGrpcClient>(options =>
{
    options.Address = new Uri(bidServiceGrpcUrl);
});

var identityServiceGrpcUrl = builder.Configuration["GrpcServices:IdentityService"] ?? "http://localhost:7010";
builder.Services.AddGrpcClient<UserStatsGrpc.UserStatsGrpcClient>(options =>
{
    options.Address = new Uri(identityServiceGrpcUrl);
});

var paymentServiceGrpcUrl = builder.Configuration["GrpcServices:PaymentService"] ?? "http://localhost:7008";
builder.Services.AddGrpcClient<PaymentAnalyticsGrpc.PaymentAnalyticsGrpcClient>(options =>
{
    options.Address = new Uri(paymentServiceGrpcUrl);
});

builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IPlatformSettingRepository, PlatformSettingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDomainEvents(typeof(UnitOfWork).Assembly);

builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPlatformSettingService, PlatformSettingService>();
builder.Services.AddScoped<IDashboardStatsService, DashboardStatsService>();
builder.Services.AddScoped<IAnalyticsService, PlatformAnalyticsService>();
builder.Services.AddScoped<IUserAnalyticsAggregator, UserAnalyticsAggregator>();

builder.Services.AddUtilityScheduling(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuditEventConsumer>();
    x.AddConsumer<AuctionCreatedAnalyticsConsumer>();
    x.AddConsumer<AuctionFinishedAnalyticsConsumer>();
    x.AddConsumer<BidPlacedAnalyticsConsumer>();
    x.AddConsumer<PaymentCompletedAnalyticsConsumer>();
    x.AddConsumer<OrderCreatedAnalyticsConsumer>();
    x.AddConsumer<OrderShippedAnalyticsConsumer>();
    x.AddConsumer<OrderDeliveredAnalyticsConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("audit-event-queue", e =>
        {
            e.ConfigureConsumer<AuditEventConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(100, 500, 1000));
        });

        cfg.ReceiveEndpoint("analytics-auction-events", e =>
        {
            e.ConfigureConsumer<AuctionCreatedAnalyticsConsumer>(context);
            e.ConfigureConsumer<AuctionFinishedAnalyticsConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(100, 500, 1000));
        });

        cfg.ReceiveEndpoint("analytics-bid-events", e =>
        {
            e.ConfigureConsumer<BidPlacedAnalyticsConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(100, 500, 1000));
        });

        cfg.ReceiveEndpoint("analytics-payment-events", e =>
        {
            e.ConfigureConsumer<PaymentCompletedAnalyticsConsumer>(context);
            e.ConfigureConsumer<OrderCreatedAnalyticsConsumer>(context);
            e.ConfigureConsumer<OrderShippedAnalyticsConsumer>(context);
            e.ConfigureConsumer<OrderDeliveredAnalyticsConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(100, 500, 1000));
        });

        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

var identityAuthority = builder.Configuration["Identity:Authority"] ?? "http://localhost:5001";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = identityAuthority;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = identityAuthority,
        ValidateAudience = false,
        NameClaimType = "username",
        RoleClaimType = "role"
    };
});

builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();

app.Run();
