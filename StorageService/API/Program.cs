using StorageService.API.Consumers;
using StorageService.API.Services;
using StorageService.API.Jobs;
using StorageService.Infrastructure.Data;
using StorageService.Infrastructure.Extensions;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Core.Authorization;
using Common.CQRS.Extensions;
using Common.Messaging.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddDomainEvents(typeof(StorageService.Infrastructure.Repositories.UnitOfWork).Assembly);

var applicationAssembly = typeof(StorageService.Application.DTOs.FileMetadataDto).Assembly;
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

builder.Services.AddMassTransitWithRabbitMq(builder.Configuration, x =>
{
    x.AddConsumer<AuctionDeletedConsumer>();
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(TempFileCleanupJob.JobId);
    q.AddJob<TempFileCleanupJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity($"{TempFileCleanupJob.JobId}-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInHours(1)
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddGrpc();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

var identityAuthority = builder.Configuration["Identity:Authority"];
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy($"Permission:{Permissions.Storage.View}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Storage.View)));
    options.AddPolicy($"Permission:{Permissions.Storage.Upload}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Storage.Upload)));
    options.AddPolicy($"Permission:{Permissions.Storage.Delete}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Storage.Delete)));
});

builder.Services.AddPermissionBasedAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
    db.Database.Migrate();
}

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseAppExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<StorageGrpcService>();

app.Run();
