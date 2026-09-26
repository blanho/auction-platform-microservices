using System.Reflection;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Application.Interfaces;
using Notification.Application.Services;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Xunit;

namespace Notification.Application.Tests;

public class NotificationSenderTests
{
    [Theory]
    [InlineData(49, true)]
    [InlineData(50, true)]
    [InlineData(51, true)]
    [InlineData(51, false)]
    public async Task Sms_PreservesPreviewAndPersistsDeliveryOutcome(int length, bool success)
    {
        var message = new string('x', length);
        NotificationRecord? persisted = null;
        var calls = new List<string>();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var template = NotificationTemplate.Create("test", "Test", "Subject", "Body", smsBody: message);
        var templates = Stub<ITemplateRepository>((_, args) =>
        {
            Assert.Equal(token, args[^1]);
            return Task.FromResult<NotificationTemplate?>(template);
        });
        var smsSender = Stub<ISmsSender>((_, args) =>
        {
            Assert.Equal(message, args[1]);
            Assert.Equal(token, args[^1]);
            calls.Add("send");
            return Task.FromResult(new SmsSendResult(success, "message-id", success ? null : "delivery failed"));
        });
        var records = Stub<INotificationRecordRepository>((method, args) =>
        {
            Assert.Equal(nameof(INotificationRecordRepository.AddRecordAsync), method.Name);
            Assert.Equal(token, args[^1]);
            persisted = Assert.IsType<NotificationRecord>(args[0]);
            calls.Add("record");
            return Task.CompletedTask;
        });
        var unit = Stub<IUnitOfWork>((method, args) =>
        {
            Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
            Assert.Equal(token, args[^1]);
            calls.Add("save");
            return Task.FromResult(1);
        });
        var sender = new NotificationSender(templates, records, unit, null!, smsSender, null!, null!,
            NullLogger<NotificationSender>.Instance);

        await sender.SendSmsAsync("invalid-user-id", "test", [], "+1234567890", token);

        Assert.Equal(new[] { "send", "record", "save" }, calls);
        Assert.NotNull(persisted);
        Assert.Equal(Guid.Empty, persisted.UserId);
        Assert.Equal(length > 50 ? new string('x', 50) + "..." : message, persisted.Subject);
        Assert.Equal(success ? NotificationRecordStatus.Sent : NotificationRecordStatus.Failed, persisted.Status);
        Assert.Equal(success ? "message-id" : null, persisted.ExternalId);
        Assert.Equal(success ? null : "delivery failed", persisted.ErrorMessage);
    }

    [Fact]
    public async Task Email_PersistenceFailurePropagatesAfterDelivery()
    {
        var template = NotificationTemplate.Create("test", "Test", "Subject", "Body");
        var templates = Stub<ITemplateRepository>((_, _) => Task.FromResult<NotificationTemplate?>(template));
        var calls = new List<string>();
        var emailSender = Stub<IEmailSender>((_, _) =>
        {
            calls.Add("send");
            return Task.FromResult(new EmailSendResult(true, "message-id"));
        });
        var records = Stub<INotificationRecordRepository>((_, _) =>
        {
            calls.Add("record");
            return Task.CompletedTask;
        });
        var failure = new InvalidOperationException("Database unavailable");
        var unit = Stub<IUnitOfWork>((_, _) =>
        {
            calls.Add("save");
            return Task.FromException<int>(failure);
        });
        var sender = new NotificationSender(templates, records, unit, emailSender, null!, null!, null!,
            NullLogger<NotificationSender>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sender.SendEmailAsync(Guid.NewGuid().ToString(), "test", [], "user@example.com"));

        Assert.Same(failure, exception);
        Assert.Equal(new[] { "send", "record", "save" }, calls);
    }

    private static T Stub<T>(Func<MethodInfo, object?[], object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[], object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args!);
    }
}
