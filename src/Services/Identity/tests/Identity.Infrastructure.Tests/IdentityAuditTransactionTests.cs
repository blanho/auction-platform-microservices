using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Infrastructure.Auditing;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;

namespace Identity.Infrastructure.Tests;

public class IdentityAuditTransactionTests
{
    [PostgresFact]
    public async Task InternalUserSaveAndAuditOutboxCommitTogether()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var services = Services(database.ConnectionString);
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        var publisher = Publisher(scope.ServiceProvider);
        var user = new ApplicationUser { UserName = "audited-user" };
        var behavior = new IdentityCommandTransactionBehavior<TestCommand, Result>(db);

        await behavior.Handle(new TestCommand(), async () =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            await publisher.PublishAsync(Guid.Parse(user.Id), new { Action = "Login" }, AuditAction.Updated,
                actor: new AuditActor(Guid.Parse(user.Id), user.UserName));
            await using var observer = Open(database.ConnectionString);
            Assert.False(await observer.Users.AnyAsync());
            Assert.False(await observer.Set<OutboxMessage>().AnyAsync());
            return Result.Success();
        }, CancellationToken.None);

        await using var committed = Open(database.ConnectionString);
        Assert.Equal(user.Id, (await committed.Users.SingleAsync()).Id);
        var message = await committed.Set<OutboxMessage>().SingleAsync();
        Assert.Contains("audited-user", message.Body);
        Assert.Contains(user.Id, message.Body);
    }

    [PostgresFact]
    public async Task ExceptionAfterInternalSaveRollsBackUserAndAudit()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var services = Services(database.ConnectionString);
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        var publisher = Publisher(scope.ServiceProvider);
        var behavior = new IdentityCommandTransactionBehavior<TestCommand, Result>(db);
        var calls = 0;

        await Assert.ThrowsAsync<IOException>(() => behavior.Handle(new TestCommand(), async () =>
        {
            calls++;
            var user = new ApplicationUser { UserName = "rolled-back" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            await publisher.PublishAsync(Guid.Parse(user.Id), new { Action = "Register" }, AuditAction.Created);
            await db.SaveChangesAsync();
            throw new IOException("failure after audit save");
        }, CancellationToken.None));

        Assert.Equal(1, calls);
        await using var observer = Open(database.ConnectionString);
        Assert.False(await observer.Users.AnyAsync());
        Assert.False(await observer.Set<OutboxMessage>().AnyAsync());
    }

    [PostgresFact]
    public async Task FailedLoginResultStillCommitsLockoutCounter()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var services = Services(database.ConnectionString);
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        var user = new ApplicationUser { UserName = "failed-login" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var behavior = new IdentityCommandTransactionBehavior<TestCommand, Result>(db);
        var result = await behavior.Handle(new TestCommand(), async () =>
        {
            user.AccessFailedCount++;
            await db.SaveChangesAsync();
            return Result.Failure(Error.Create("Auth.Invalid", "Invalid credentials"));
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        await using var observer = Open(database.ConnectionString);
        Assert.Equal(1, (await observer.Users.SingleAsync()).AccessFailedCount);
    }

    private sealed record TestCommand : ICommand;

    private static ApplicationDbContext Open(string connection) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);

    private static ServiceProvider Services(string connection)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connection, postgres => postgres.EnableRetryOnFailure()));
        services.AddMassTransit(configure =>
        {
            configure.AddEntityFrameworkOutbox<ApplicationDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });
            configure.UsingInMemory();
        });
        return services.BuildServiceProvider();
    }

    private static AuditPublisher Publisher(IServiceProvider services) => new(
        services.GetRequiredService<IPublishEndpoint>(),
        TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? Guid.Empty : null),
        NullLogger<AuditPublisher>.Instance, "identity-service");
}
