using Common.Storage.Abstractions;
using Common.Storage.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UtilityService.BackgroundServices;
using UtilityService.Consumers;
using UtilityService.Data;
using UtilityService.GrpcServices;
using UtilityService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTP/1.1 (REST) and HTTP/2 (gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/1.1 endpoint for REST API
    options.ListenLocalhost(5005, o => o.Protocols = HttpProtocols.Http1);
    // HTTP/2 endpoint for gRPC
    options.ListenLocalhost(5006, o => o.Protocols = HttpProtocols.Http2);
});

// Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Database
builder.Services.AddDbContext<UtilityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Storage services
builder.Services.AddStorageServices(builder.Configuration);
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Background services
builder.Services.AddHostedService<TempFileCleanupService>();

// gRPC services
builder.Services.AddGrpc();

// MassTransit for consuming audit events
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UtilityDbContext>();
    db.Database.Migrate();
    
    // Ensure upload directories exist
    var uploadPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
    Directory.CreateDirectory(Path.Combine(uploadPath, "temp"));
    Directory.CreateDirectory(Path.Combine(uploadPath, "files"));
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve uploaded files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "uploads")),
    RequestPath = "/files"
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// gRPC endpoints
app.MapGrpcService<FileStorageGrpcService>();

app.MapControllers();

app.Run();
