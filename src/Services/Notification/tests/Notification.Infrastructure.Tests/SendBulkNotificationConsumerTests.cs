using System.Reflection;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Messaging.Consumers;
using Notification.Infrastructure.Senders;
using NotificationService.Contracts.Commands;
using NotificationService.Contracts.Events;
using Xunit;

namespace Notification.Infrastructure.Tests;

public class SendBulkNotificationConsumerTests
{
    [Fact]
    public async Task MultipleBatches_ReportOnlyEachBatchProgress()
    {
        var message = new SendBulkNotificationCommand
        {
            CorrelationId = Guid.NewGuid(),
            BatchSize = 1,
            Recipients =
            [
                new BulkNotificationRecipient { UserId = Guid.NewGuid() },
                new BulkNotificationRecipient { UserId = Guid.NewGuid() }
            ]
        };
        var published = new List<object>();
        var context = Stub<ConsumeContext<SendBulkNotificationCommand>>((method, args) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => Record(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        var publisher = Stub<IPublishEndpoint>((method, args) => method.Name switch
        {
            "Publish" => Record(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task Record(object?[] args)
        {
            published.Add(args[0]!);
            return Task.CompletedTask;
        }

        var idempotency = Stub<IIdempotencyService>((method, _) => method.Name switch
        {
            nameof(IIdempotencyService.IsProcessedAsync) => Task.FromResult(false),
            nameof(IIdempotencyService.MarkAsProcessedAsync) => Task.CompletedTask,
            _ => throw new NotSupportedException(method.Name)
        });
        var templates = Stub<ITemplateRepository>((method, _) => method.Name switch
        {
            nameof(ITemplateRepository.GetByKeyAsync) => Task.FromResult<NotificationTemplate?>(null),
            _ => throw new NotSupportedException(method.Name)
        });
        var consumer = new SendBulkNotificationConsumer(idempotency, templates, null!, null!, null!,
            null!, null!, null!, publisher, NullLogger<SendBulkNotificationConsumer>.Instance);

        await consumer.Consume(context);

        var progress = published.OfType<ReportJobBatchProgressCommand>().ToList();
        Assert.Equal(2, progress.Count);
        Assert.All(progress, update =>
        {
            Assert.Equal(1, update.CompletedCount);
            Assert.Equal(0, update.FailedCount);
        });
        var completion = Assert.Single(published.OfType<BulkNotificationCompletedEvent>());
        Assert.Equal(2, completion.SuccessCount);
        Assert.Equal(0, completion.FailureCount);
    }

    [Fact]
    public async Task CanceledRecipient_DoesNotReportFailureOrCompletion()
    {
        using var cancellation = new CancellationTokenSource();
        var published = new List<object>();
        var message = new SendBulkNotificationCommand
        {
            CorrelationId = Guid.NewGuid(),
            Recipients = [new BulkNotificationRecipient { UserId = Guid.NewGuid() }]
        };
        var context = Stub<ConsumeContext<SendBulkNotificationCommand>>((method, args) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => cancellation.Token,
            "Publish" => Record(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        var publisher = Stub<IPublishEndpoint>((method, args) => method.Name switch
        {
            "Publish" => Record(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task Record(object?[] args)
        {
            published.Add(args[0]!);
            return Task.CompletedTask;
        }

        var idempotency = Stub<IIdempotencyService>((method, _) => method.Name switch
        {
            nameof(IIdempotencyService.IsProcessedAsync) => CancelRecipient(),
            _ => throw new NotSupportedException(method.Name)
        });
        async Task<bool> CancelRecipient()
        {
            await cancellation.CancelAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            return false;
        }
        var templates = Stub<ITemplateRepository>((method, _) => method.Name switch
        {
            nameof(ITemplateRepository.GetByKeyAsync) => Task.FromResult<NotificationTemplate?>(null),
            _ => throw new NotSupportedException(method.Name)
        });
        var consumer = new SendBulkNotificationConsumer(idempotency, templates, null!, null!, null!,
            null!, null!, null!, publisher, NullLogger<SendBulkNotificationConsumer>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consumer.Consume(context));
        Assert.Empty(published.OfType<ReportJobBatchProgressCommand>());
        Assert.Empty(published.OfType<BulkNotificationCompletedEvent>());
    }

    [Fact]
    public async Task DefaultSenders_PropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new EmailSender(NullLogger<EmailSender>.Instance).SendAsync(
                "user@example.com", "Subject", "Body", ct: cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new SmsNotificationSender(NullLogger<SmsNotificationSender>.Instance).SendAsync(
                "+1234567890", "Message", cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new PushSender(NullLogger<PushSender>.Instance).SendAsync(
                "user", "Title", "Body", ct: cancellation.Token));
    }

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
