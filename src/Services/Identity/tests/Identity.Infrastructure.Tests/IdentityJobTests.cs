using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Localization;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Infrastructure.Jobs;
using Identity.Infrastructure.Persistence;
using IdentityService.Contracts.Events;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TestSupport;
using Xunit;

namespace Identity.Infrastructure.Tests;

public sealed class IdentityJobTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

    [PostgresFact]
    public async Task Cleanup_DeletesOnlyExpiredHistoryWithoutLiveSessions()
    {
        await using var host = await JobHost.CreateAsync();
        var idle = User("idle");
        var live = User("live");
        var old = Token(idle, Now.AddDays(-31));
        var boundary = Token(idle, Now.AddDays(-30));
        var recent = Token(idle, Now.AddDays(-29));
        var absoluteExpired = Token(idle, Now.AddDays(1));
        absoluteExpired.AbsoluteExpiration = Now.AddDays(-31);
        var revokedButUnexpired = Token(idle, Now.AddDays(1), revoked: true);
        var ancestor = Token(live, Now.AddDays(-60), revoked: true);
        var current = Token(live, Now.AddDays(1));
        ancestor.ReplacedByToken = current.Token;
        await host.SeedAsync(old, boundary, recent, absoluteExpired, revokedButUnexpired, ancestor, current);

        await host.RunAsync<RefreshTokenCleanupJob>();

        await using var db = host.OpenDb();
        var remaining = await db.RefreshTokens.Select(token => token.Id).ToListAsync();
        Assert.Equal(4, remaining.Count);
        Assert.DoesNotContain(old.Id, remaining);
        Assert.DoesNotContain(boundary.Id, remaining);
        Assert.DoesNotContain(absoluteExpired.Id, remaining);
        Assert.Contains(recent.Id, remaining);
        Assert.Contains(revokedButUnexpired.Id, remaining);
        Assert.Contains(ancestor.Id, remaining);
        Assert.Contains(current.Id, remaining);

        await db.RefreshTokens.Where(token => token.Id == current.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.IsRevoked, true));
        await host.RunAsync<RefreshTokenCleanupJob>();
        Assert.False(await db.RefreshTokens.AnyAsync(token => token.Id == ancestor.Id));
    }

    [PostgresFact]
    public async Task Cleanup_BoundsWorkAndResumesOnNextRun()
    {
        await using var host = await JobHost.CreateAsync(new IdentityJobOptions { BatchSize = 2, MaxBatchesPerRun = 2 });
        var user = User("idle");
        await host.SeedAsync(Enumerable.Range(0, 5).Select(_ => Token(user, Now.AddDays(-60))).ToArray());

        await host.RunAsync<RefreshTokenCleanupJob>();
        await using var db = host.OpenDb();
        Assert.Equal(1, await db.RefreshTokens.CountAsync());

        await host.RunAsync<RefreshTokenCleanupJob>();
        Assert.Equal(0, await db.RefreshTokens.CountAsync());
    }

    [PostgresFact]
    public async Task Revocation_OnlyAffectsDisabledAccountsAndPreservesExistingRevocation()
    {
        await using var host = await JobHost.CreateAsync();
        var inactive = User("inactive");
        inactive.IsActive = false;
        var suspended = User("suspended");
        suspended.IsSuspended = true;
        var active = User("active");
        var inactiveToken = Token(inactive, Now.AddDays(1));
        var suspendedToken = Token(suspended, Now.AddDays(1));
        var activeToken = Token(active, Now.AddDays(1));
        var alreadyRevoked = Token(inactive, Now.AddDays(1), revoked: true);
        alreadyRevoked.RevokedAt = Now.AddDays(-1);
        alreadyRevoked.RevokedByIp = "127.0.0.1";
        await host.SeedAsync(inactiveToken, suspendedToken, activeToken, alreadyRevoked);

        await host.RunAsync<DisabledUserTokenRevocationJob>();
        await host.RunAsync<DisabledUserTokenRevocationJob>();

        await using var db = host.OpenDb();
        var tokens = await db.RefreshTokens.ToDictionaryAsync(token => token.Id);
        Assert.True(tokens[inactiveToken.Id].IsRevoked);
        Assert.True(tokens[suspendedToken.Id].IsRevoked);
        Assert.Equal(Now, tokens[inactiveToken.Id].RevokedAt);
        Assert.False(tokens[activeToken.Id].IsRevoked);
        Assert.Equal(Now.AddDays(-1), tokens[alreadyRevoked.Id].RevokedAt);
        Assert.Equal("127.0.0.1", tokens[alreadyRevoked.Id].RevokedByIp);
    }

    [PostgresFact]
    public async Task Revocation_BoundsWorkAndResumesOnNextRun()
    {
        await using var host = await JobHost.CreateAsync(new IdentityJobOptions { BatchSize = 2, MaxBatchesPerRun = 2 });
        var user = User("disabled");
        user.IsActive = false;
        await host.SeedAsync(Enumerable.Range(0, 5).Select(_ => Token(user, Now.AddDays(1))).ToArray());

        await host.RunAsync<DisabledUserTokenRevocationJob>();
        await using var db = host.OpenDb();
        Assert.Equal(4, await db.RefreshTokens.CountAsync(token => token.IsRevoked));

        await host.RunAsync<DisabledUserTokenRevocationJob>();
        Assert.Equal(5, await db.RefreshTokens.CountAsync(token => token.IsRevoked));
    }

    [PostgresFact]
    public async Task DisabledJobs_DoNotChangeTokens()
    {
        await using var host = await JobHost.CreateAsync(new IdentityJobOptions
        {
            RefreshTokenCleanupEnabled = false,
            DisabledUserTokenRevocationEnabled = false
        });
        var user = User("disabled");
        user.IsActive = false;
        var token = Token(user, Now.AddDays(-60));
        await host.SeedAsync(token);

        await host.RunAsync<RefreshTokenCleanupJob>();
        await host.RunAsync<DisabledUserTokenRevocationJob>();

        await using var db = host.OpenDb();
        Assert.False((await db.RefreshTokens.SingleAsync()).IsRevoked);
    }

    [PostgresFact]
    public async Task CancelledJobs_DoNotChangeTokensOrUsers()
    {
        await using var host = await JobHost.CreateAsync();
        var user = User("inactive");
        await host.SeedAsync(Token(user, Now.AddDays(-60)));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => host.RunAsync<RefreshTokenCleanupJob>(cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => host.RunAsync<DisabledUserTokenRevocationJob>(cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => host.RunAsync<InactiveUserLockJob>(cancellation.Token));

        await using var db = host.OpenDb();
        Assert.False((await db.RefreshTokens.SingleAsync()).IsRevoked);
        Assert.True((await db.Users.SingleAsync()).IsActive);
    }

    [PostgresFact]
    public async Task Inactivity_UsesExistingThirtyDayRuleAndBoundsWork()
    {
        await using var host = await JobHost.CreateAsync(new IdentityJobOptions { BatchSize = 1, MaxBatchesPerRun = 2 });
        var eligible = Enumerable.Range(0, 3).Select(i => User($"idle-{i}")).ToArray();
        eligible[0].LastLoginAt = Now.AddDays(-31);
        var fresh = User("fresh");
        fresh.CreatedAt = Now.AddDays(-10);
        var recentLogin = User("recent-login");
        recentLogin.LastLoginAt = Now.AddDays(-1);
        var boundary = User("boundary");
        boundary.LastLoginAt = Now.AddDays(-30);
        var suspended = User("suspended");
        suspended.IsSuspended = true;
        await using (var db = host.OpenDb())
        {
            db.Users.AddRange([.. eligible, fresh, recentLogin, boundary, suspended]);
            await db.SaveChangesAsync();
        }

        await host.RunAsync<InactiveUserLockJob>();
        await using var verify = host.OpenDb();
        Assert.Equal(2, await verify.Users.CountAsync(user => !user.IsActive));
        Assert.Equal(2, host.Publisher.Events.Count);
        Assert.All(host.Publisher.Events, message => Assert.Equal(Now, message.SuspendedAt));

        await host.RunAsync<InactiveUserLockJob>();
        Assert.Equal(3, await verify.Users.CountAsync(user => !user.IsActive));
        Assert.Equal(3, host.Publisher.Events.Count);
        Assert.True(await verify.Users.Where(user => user.Id == fresh.Id || user.Id == recentLogin.Id ||
            user.Id == boundary.Id || user.Id == suspended.Id).AllAsync(user => user.IsActive));
        var locked = await verify.Users.FirstAsync(user => !user.IsActive);
        Assert.NotEqual(eligible.First(user => user.Id == locked.Id).ConcurrencyStamp, locked.ConcurrencyStamp);
        Assert.NotEqual(eligible.First(user => user.Id == locked.Id).SecurityStamp, locked.SecurityStamp);
    }

    [PostgresFact]
    public async Task Inactivity_PublicationFailureDoesNotPersistAccountLock()
    {
        await using var host = await JobHost.CreateAsync();
        await host.SeedAsync(Token(User("inactive"), Now.AddDays(1)));
        host.Publisher.OnPublish = _ => throw new InvalidOperationException("Publication failed");

        await Assert.ThrowsAsync<JobExecutionException>(() => host.RunAsync<InactiveUserLockJob>());

        await using var db = host.OpenDb();
        Assert.True((await db.Users.SingleAsync()).IsActive);
    }

    [PostgresFact]
    public async Task Inactivity_ConcurrentLoginPreventsStaleAccountLock()
    {
        await using var host = await JobHost.CreateAsync();
        var user = User("inactive");
        await host.SeedAsync(Token(user, Now.AddDays(1)));
        host.Publisher.OnPublish = async _ =>
        {
            await using var db = host.OpenDb();
            await db.Users.Where(account => account.Id == user.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(account => account.LastLoginAt, Now)
                .SetProperty(account => account.ConcurrencyStamp, Guid.NewGuid().ToString()));
        };

        var error = await Assert.ThrowsAsync<JobExecutionException>(() => host.RunAsync<InactiveUserLockJob>());

        Assert.IsType<DbUpdateConcurrencyException>(error.InnerException);
        await using var verify = host.OpenDb();
        Assert.True((await verify.Users.SingleAsync()).IsActive);
    }

    [PostgresFact]
    public async Task Inactivity_PersistsOneOutboxEventPerLockedUserAcrossBatches()
    {
        await using var host = await JobHost.CreateAsync(new IdentityJobOptions { BatchSize = 1 }, useOutbox: true);
        await host.SeedAsync(Token(User("first"), Now.AddDays(1)), Token(User("second"), Now.AddDays(1)));

        await host.RunAsync<InactiveUserLockJob>();

        await using var db = host.OpenDb();
        Assert.Equal(2, await db.Users.CountAsync(user => !user.IsActive));
        Assert.Equal(2, await db.Set<OutboxMessage>().CountAsync());
        Assert.All(await db.Set<OutboxMessage>().Select(message => message.Body).ToListAsync(),
            body => Assert.Contains("Account locked due to 30 days of inactivity", body));
    }

    [PostgresFact]
    public async Task Inactivity_ConcurrencyFailureRollsBackOutboxAndAccountChanges()
    {
        await using var host = await JobHost.CreateAsync(useOutbox: true);
        var user = User("inactive");
        await host.SeedAsync(Token(user, Now.AddDays(1)));
        host.Publisher.OnPublish = async _ =>
        {
            await using var db = host.OpenDb();
            await db.Users.Where(account => account.Id == user.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(account => account.LastLoginAt, Now)
                .SetProperty(account => account.ConcurrencyStamp, Guid.NewGuid().ToString()));
        };

        await Assert.ThrowsAsync<JobExecutionException>(() => host.RunAsync<InactiveUserLockJob>());

        await using var verify = host.OpenDb();
        Assert.True((await verify.Users.SingleAsync()).IsActive);
        Assert.Equal(0, await verify.Set<OutboxMessage>().CountAsync());
        Assert.Equal(0, await verify.Set<OutboxState>().CountAsync());
    }

    [PostgresFact]
    public async Task DisabledAccounts_CannotIssueOrRefreshTokensBeforeOrAfterMaintenance()
    {
        await using var host = await JobHost.CreateAsync();
        var inactive = User("inactive");
        inactive.IsActive = false;
        var suspended = User("suspended");
        suspended.IsSuspended = true;
        var first = Token(inactive, Now.AddDays(1), raw: "inactive-token");
        var second = Token(suspended, Now.AddDays(1), raw: "suspended-token");
        await host.SeedAsync(first, second);

        foreach (var raw in new[] { "inactive-token", "suspended-token" })
        {
            await using var scope = host.Services.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TokenGenerationService>();
            var userId = raw == "inactive-token" ? inactive.Id : suspended.Id;
            Assert.Null(await service.GenerateTokensForUserAsync(userId, "nextApp"));
            Assert.Null(await service.GenerateTokenPairAsync(userId, "nextApp"));
            Assert.False((await service.RefreshTokenAsync(raw, "nextApp")).IsSuccess);
        }

        await host.RunAsync<DisabledUserTokenRevocationJob>();
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<TokenGenerationService>();
            Assert.False((await service.RefreshTokenAsync("inactive-token", "nextApp")).IsSuccess);
            Assert.False((await service.RefreshTokenAsync("suspended-token", "nextApp")).IsSuccess);
        }

        Assert.Equal(0, host.SecurityAlerts);
        await using var db = host.OpenDb();
        Assert.Equal(2, await db.RefreshTokens.CountAsync());
        Assert.True(await db.RefreshTokens.AllAsync(token => token.IsRevoked));
    }

    [PostgresFact]
    public async Task ActiveAccounts_CanRotateTokensAndRetainReplayDetectionAfterCleanup()
    {
        await using var host = await JobHost.CreateAsync();
        var user = User("active");
        await host.SeedAsync(Token(user, Now.AddDays(-60), revoked: true, raw: "old-token"));
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<TokenGenerationService>();
            Assert.NotNull(await service.GenerateTokensForUserAsync(user.Id, "nextApp"));
            var pair = await service.GenerateTokenPairAsync(user.Id, "nextApp");
            Assert.NotNull(pair);
            Assert.True((await service.RefreshTokenAsync(pair.Value.RefreshToken, "nextApp")).IsSuccess);
        }

        await host.RunAsync<RefreshTokenCleanupJob>();
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<TokenGenerationService>();
            Assert.Equal(RefreshTokenFailureReason.SecurityTermination,
                (await service.RefreshTokenAsync("old-token", "nextApp")).FailureReason);
        }

        Assert.Equal(1, host.SecurityAlerts);
        await using var db = host.OpenDb();
        Assert.True(await db.RefreshTokens.AllAsync(token => token.IsRevoked));
    }

    [Fact]
    public async Task Registration_RegistersAllJobsAndValidatesLimits()
    {
        var services = new ServiceCollection().AddLogging().AddIdentityJobs(new ConfigurationBuilder().Build());
        await using var provider = services.BuildServiceProvider();
        var scheduler = await provider.GetRequiredService<ISchedulerFactory>().GetScheduler();
        Assert.True(await scheduler.CheckExists(new JobKey(InactiveUserLockJob.JobId)));
        Assert.True(await scheduler.CheckExists(new JobKey(RefreshTokenCleanupJob.JobId)));
        Assert.True(await scheduler.CheckExists(new JobKey(DisabledUserTokenRevocationJob.JobId)));
        var trigger = Assert.IsAssignableFrom<ISimpleTrigger>((await scheduler.GetTriggersOfJob(
            new JobKey(DisabledUserTokenRevocationJob.JobId))).Single());
        Assert.Equal(TimeSpan.FromMinutes(5), trigger.RepeatInterval);
        await scheduler.Shutdown();

        foreach (var key in new[] { "BatchSize", "MaxBatchesPerRun", "ExpiredTokenRetentionDays" })
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { [$"IdentityJobs:{key}"] = "0" }).Build();
            await using var invalid = new ServiceCollection().AddLogging().AddIdentityJobs(configuration).BuildServiceProvider();
            Assert.Throws<OptionsValidationException>(() => invalid.GetRequiredService<IOptions<IdentityJobOptions>>().Value);
        }
    }

    [Fact]
    public void InactivityReason_CanBeLocalizedToJapanese()
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            Assert.Equal("30日間ログインがなかったため、アカウントがロックされました",
                UserMessageLocalizer.Translate("Account locked due to 30 days of inactivity"));
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    private static ApplicationUser User(string name) => new()
    {
        Id = Guid.NewGuid().ToString(), UserName = name, NormalizedUserName = name.ToUpperInvariant(),
        CreatedAt = Now.AddDays(-60), SecurityStamp = Guid.NewGuid().ToString(),
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };

    private static RefreshToken Token(ApplicationUser user, DateTimeOffset expires, bool revoked = false, string? raw = null) => new()
    {
        Id = Guid.NewGuid(), User = user, UserId = user.Id,
        Token = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw ?? Guid.NewGuid().ToString()))),
        JwtId = Guid.NewGuid().ToString(), CreatedAt = Now.AddDays(-90), ExpiresAt = expires, IsRevoked = revoked
    };

    private sealed class JobHost(PostgresTestDatabase database) : IAsyncDisposable
    {
        private string ConnectionString => database.ConnectionString;
        public ServiceProvider Services { get; private set; } = null!;
        public RecordingPublisher Publisher { get; } = new();
        public int SecurityAlerts { get; private set; }

        public static async Task<JobHost> CreateAsync(IdentityJobOptions? options = null, bool useOutbox = false)
        {
            var host = new JobHost(await PostgresTestDatabase.CreateAsync());
            var services = new ServiceCollection().AddLogging();
            services.AddDbContext<ApplicationDbContext>(builder => builder.UseNpgsql(host.ConnectionString,
                postgres => postgres.EnableRetryOnFailure()));
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddSingleton<IOptions<IdentityJobOptions>>(Options.Create(options ?? new IdentityJobOptions()));
            services.AddSingleton<TimeProvider>(new FixedTimeProvider());
            if (useOutbox)
            {
                services.AddMassTransit(configure =>
                {
                    configure.AddEntityFrameworkOutbox<ApplicationDbContext>(outbox =>
                    {
                        outbox.UsePostgres();
                        outbox.UseBusOutbox();
                    });
                    configure.UsingInMemory();
                });
                services.AddScoped<IEventPublisher>(provider => new ScopedOutboxPublisher(
                    host.Publisher, provider.GetRequiredService<IPublishEndpoint>()));
            }
            else
            {
                services.AddSingleton<IEventPublisher>(host.Publisher);
            }
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Identity:SecretKey"] = "identity-maintenance-tests-signing-key-256-bits-minimum"
            }).Build());
            services.AddSingleton(TestProxy.Create<IMediator>((method, _) =>
            {
                if (method.Name != "Publish") throw new NotSupportedException(method.Name);
                host.SecurityAlerts++;
                return Task.CompletedTask;
            }));
            services.AddSingleton(TestProxy.Create<IRolePermissionService>((method, _) =>
                method.Name == nameof(IRolePermissionService.GetPermissionsForRolesAsync)
                    ? Task.FromResult(new HashSet<string>()) : throw new NotSupportedException(method.Name)));
            services.AddScoped<TokenGenerationService>();
            host.Services = services.BuildServiceProvider();
            await using var db = host.OpenDb();
            await db.Database.MigrateAsync();
            if (useOutbox)
                await host.Services.GetRequiredService<IBusControl>().StartAsync();
            return host;
        }

        public ApplicationDbContext OpenDb() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(database.ConnectionString).Options);

        public async Task SeedAsync(params RefreshToken[] tokens)
        {
            await using var db = OpenDb();
            db.RefreshTokens.AddRange(tokens);
            await db.SaveChangesAsync();
        }

        public Task RunAsync<TJob>(CancellationToken cancellationToken = default) where TJob : class, IJob
        {
            var job = ActivatorUtilities.CreateInstance<TJob>(Services);
            var context = TestProxy.Create<IJobExecutionContext>((method, _) => method.Name switch
            {
                "get_JobDetail" => JobBuilder.Create<TJob>().WithIdentity(typeof(TJob).Name).Build(),
                "get_FireTimeUtc" => Now,
                "get_RefireCount" => 3,
                "get_CancellationToken" => cancellationToken,
                _ => throw new NotSupportedException(method.Name)
            });
            return job.Execute(context);
        }

        public async ValueTask DisposeAsync()
        {
            var bus = Services.GetService<IBusControl>();
            if (bus != null) await bus.StopAsync();
            await Services.DisposeAsync();
            await database.DisposeAsync();
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class RecordingPublisher : IEventPublisher
    {
        public List<UserSuspendedEvent> Events { get; } = [];
        public Func<UserSuspendedEvent, Task>? OnPublish { get; set; }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            var suspended = Assert.IsType<UserSuspendedEvent>(message);
            if (OnPublish != null) await OnPublish(suspended);
            Events.Add(suspended);
        }

        public Task PublishBatchAsync<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default) where T : class =>
            throw new NotSupportedException();
    }

    private sealed class ScopedOutboxPublisher(RecordingPublisher hooks, IPublishEndpoint endpoint) : IEventPublisher
    {
        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            await hooks.PublishAsync(message, cancellationToken);
            await endpoint.Publish(message, cancellationToken);
        }

        public Task PublishBatchAsync<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default) where T : class =>
            throw new NotSupportedException();
    }
}
