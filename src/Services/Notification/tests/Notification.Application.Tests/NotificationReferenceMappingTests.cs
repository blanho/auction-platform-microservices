using System.Reflection;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Features.Notifications.CreateNotification;
using Notification.Application.Interfaces;
using Xunit;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Tests;

public sealed class NotificationReferenceMappingTests
{
    [Fact]
    public async Task CreateNotification_KeepsAuctionAndBidReferencesInTheirOwnFields()
    {
        NotificationEntity? persisted = null;
        var repository = Stub<INotificationRepository>((_, args) =>
        {
            persisted = Assert.IsType<NotificationEntity>(args![0]);
            return Task.FromResult(persisted);
        });
        var unit = Stub<IUnitOfWork>((_, _) => Task.FromResult(1));
        var mapper = Stub<IMapper>((_, _) => new NotificationDto());
        var hub = Stub<INotificationHubService>((_, _) => Task.CompletedTask);
        var audit = Stub<IAuditPublisher>((_, _) => Task.CompletedTask);
        var request = new CreateNotificationCommand(Guid.NewGuid().ToString(), "General", "title", "message",
            AuctionId: Guid.NewGuid(), BidId: Guid.NewGuid());
        var handler = new CreateNotificationCommandHandler(repository, unit, mapper, hub,
            NullLogger<CreateNotificationCommandHandler>.Instance, audit);

        var result = await handler.Handle(request, default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(persisted);
        Assert.Equal(request.AuctionId, persisted.AuctionId);
        Assert.Equal(request.BidId, persisted.BidId);
        Assert.Null(persisted.OrderId);
    }

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, Proxy>();
        ((Proxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class Proxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    }
}
