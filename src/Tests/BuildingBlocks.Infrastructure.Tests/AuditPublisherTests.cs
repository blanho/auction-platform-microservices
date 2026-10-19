using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Auditing;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using System.Text.Json;
using TestSupport;
using Xunit;
using AuditAction = BuildingBlocks.Application.Abstractions.Auditing.AuditAction;

namespace BuildingBlocks.Infrastructure.Tests;

public class AuditPublisherTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Publish_UsesVerifiedActorWhenProvidedAndPreservesChanges(bool overrideActor)
    {
        var requestUser = Guid.NewGuid();
        var verifiedUser = Guid.NewGuid();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim("sub", requestUser.ToString()), new Claim("username", "request-user")]))
        };
        var auditContext = new HttpAuditContext(new HttpContextAccessor { HttpContext = context },
            TestProxy.Create<ICorrelationIdProvider>((_, _) => "correlation"));
        AuditEvent? captured = null;
        using var cancellation = new CancellationTokenSource();
        var endpoint = TestProxy.Create<IPublishEndpoint>((_, args) =>
        {
            captured = Assert.IsType<AuditEvent>(args![0]);
            Assert.Equal(cancellation.Token, args[^1]);
            return Task.CompletedTask;
        });
        var publisher = new AuditPublisher(endpoint, auditContext, NullLogger<AuditPublisher>.Instance, "auction-service");
        var entityId = Guid.NewGuid();
        await publisher.PublishAsync(entityId, new Snapshot("new"), AuditAction.Updated,
            new Snapshot("old"), new() { ["Reason"] = "correction" }, cancellation.Token,
            overrideActor ? new AuditActor(verifiedUser, "verified-user") : null);

        Assert.NotNull(captured);
        Assert.Equal(overrideActor ? verifiedUser : requestUser, captured.UserId);
        Assert.Equal(overrideActor ? "verified-user" : "request-user", captured.Username);
        Assert.Equal(entityId, captured.EntityId);
        Assert.Equal("auction-service", captured.ServiceName);
        Assert.Equal("correlation", captured.CorrelationId);
        Assert.Equal(["Name"], captured.ChangedProperties);
        Assert.Equal("old", JsonDocument.Parse(captured.OldValues!).RootElement.GetProperty("name").GetString());
        Assert.Equal("new", JsonDocument.Parse(captured.NewValues!).RootElement.GetProperty("name").GetString());
        Assert.Equal("correction", captured.Metadata!["Reason"]);
    }

    [Fact]
    public async Task Publish_AttributesAnonymousAuthenticationToVerifiedUser()
    {
        var actor = new AuditActor(Guid.NewGuid(), "signed-in-user");
        AuditEvent? captured = null;
        var context = new HttpAuditContext(new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            TestProxy.Create<ICorrelationIdProvider>((_, _) => null));
        var publisher = new AuditPublisher(TestProxy.Create<IPublishEndpoint>((_, args) =>
        {
            captured = (AuditEvent)args![0]!;
            return Task.CompletedTask;
        }), context, NullLogger<AuditPublisher>.Instance, "identity-service");

        await publisher.PublishAsync(actor.UserId, new Snapshot("login"), AuditAction.Updated, actor: actor);

        Assert.Equal(actor.UserId, captured!.UserId);
        Assert.Equal(actor.Username, captured.Username);
    }

    private sealed record Snapshot(string Name);
}
