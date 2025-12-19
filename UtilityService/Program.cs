using Common.Messaging.Abstractions;
using Common.Messaging.Implementations;
using Common.Resilience;
using Common.Resilience.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UtilityService.Consumers;
using UtilityService.Data;
using UtilityService.Extensions;
using UtilityService.Grpc;
using UtilityService.Interfaces;
using UtilityService.Repositories;
using UtilityService.Services;

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

builder.Services.AddDbContext<UtilityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddResiliencePolicies(builder.Configuration);
var resilienceOptions = builder.Configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>() ?? new ResilienceOptions();

var auctionServiceGrpcUrl = builder.Configuration["GrpcServices:AuctionService"] ?? "http://localhost:7002";
builder.Services.AddGrpcClient<AuctionGrpc.AuctionGrpcClient>(options =>
{
    options.Address = new Uri(auctionServiceGrpcUrl);
}).AddResiliencePolicies(resilienceOptions);

var bidServiceGrpcUrl = builder.Configuration["GrpcServices:BidService"] ?? "http://localhost:7004";
builder.Services.AddGrpcClient<BidStatsGrpc.BidStatsGrpcClient>(options =>
{
    options.Address = new Uri(bidServiceGrpcUrl);
}).AddResiliencePolicies(resilienceOptions);

var paymentServiceGrpcUrl = builder.Configuration["GrpcServices:PaymentService"] ?? "http://localhost:7008";
builder.Services.AddGrpcClient<PaymentAnalyticsGrpc.PaymentAnalyticsGrpcClient>(options =>
{
    options.Address = new Uri(paymentServiceGrpcUrl);
}).AddResiliencePolicies(resilienceOptions);

builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IPlatformSettingRepository, PlatformSettingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPlatformSettingService, PlatformSettingService>();
builder.Services.AddScoped<IDashboardStatsService, DashboardStatsService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddUtilityScheduling(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuditEventConsumer>();

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
    options.RequireHttpsMetadata = false;
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

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UtilityDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();

app.Run();
