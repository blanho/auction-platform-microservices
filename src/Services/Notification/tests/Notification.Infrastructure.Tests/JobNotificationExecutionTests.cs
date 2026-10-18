using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Localization;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Application.Interfaces;
using Notification.Domain.Constants;
using Notification.Domain.Entities;
using Notification.Infrastructure.Messaging.Consumers;
using NotificationService.Contracts.Commands;
using NotificationService.Contracts.Events;
using TestSupport;
using Xunit;

namespace Notification.Infrastructure.Tests;

public class JobNotificationExecutionTests
{
    [Fact]
    public async Task Retry_PreservesSuccessfulChannelsAndRetriesFailedDelivery()
    {
        var fixture = new Fixture();
        fixture.PushSucceeds = false;
        await fixture.Consume(fixture.Command);
        var failure = Assert.Single(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.False(failure.IsSuccess);
        Assert.True(failure.IsFinalFailure);
        Assert.Equal(fixture.Command.ParentJobId, failure.JobId);
        Assert.Equal(fixture.Command.ParentJobItemId, failure.JobItemId);
        Assert.Equal(0, failure.Attempt);
        Assert.Equal(1, fixture.EmailSends);
        Assert.Equal(1, fixture.PushSends);

        fixture.Published.Clear();
        fixture.PushSucceeds = true;
        var retry = fixture.Command with { CorrelationId = Guid.NewGuid(), Attempt = 1 };
        await fixture.Consume(retry);
        var success = Assert.Single(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.True(success.IsSuccess);
        Assert.Equal(1, success.Attempt);
        Assert.Equal(1, fixture.EmailSends);
        Assert.Equal(2, fixture.PushSends);
        Assert.Empty(fixture.Published.OfType<RequestJobCommand>());
        Assert.Empty(fixture.Published.OfType<ReportJobBatchProgressCommand>());
        Assert.Equal(1, Assert.Single(fixture.Published.OfType<BulkNotificationCompletedEvent>()).SuccessCount);

        await fixture.Consume(retry);
        Assert.Equal(1, fixture.EmailSends);
        Assert.Equal(2, fixture.PushSends);
    }

    [Fact]
    public async Task DifferentItems_DoNotShareDeliveryReceipts()
    {
        var fixture = new Fixture();
        await fixture.Consume(fixture.Command);
        await fixture.Consume(fixture.Command with { ParentJobItemId = Guid.NewGuid(), CorrelationId = Guid.NewGuid() });
        Assert.Equal(2, fixture.EmailSends);
        Assert.Equal(2, fixture.PushSends);
    }

    [Theory]
    [InlineData("empty-recipients", "有効なユーザーIDを含む、件数上限内の空でない宛先リストは必須です。")]
    [InlineData("null-recipients", "有効なユーザーIDを含む、件数上限内の空でない宛先リストは必須です。")]
    [InlineData("duplicate-recipients", "同じジョブ項目内の宛先ユーザーIDは重複できません。")]
    [InlineData("invalid-batch", "バッチサイズは10から500の間である必要があります。")]
    [InlineData("unknown-channel", "対応している通知チャネルを指定してください。")]
    [InlineData("null-channels", "対応している通知チャネルを指定してください。")]
    [InlineData("missing-address", "すべてのメール宛先にメールアドレスが必要です。")]
    [InlineData("scheduled", "ジョブから実行する通知では配信日時を指定できません。配信時刻になってからジョブを送信してください。")]
    [InlineData("missing-requester", "ジョブ項目とリクエスト者は必須です。")]
    [InlineData("missing-template", "テンプレートキー、タイトル、メッセージ、共通パラメーターは必須です。")]
    [InlineData("missing-sms-number", "すべてのSMS宛先に電話番号が必要です。")]
    public async Task InvalidPayload_FailsWithoutSending(string scenario, string japanese)
    {
        var fixture = new Fixture();
        var command = scenario switch
        {
            "empty-recipients" => fixture.Command with { Recipients = [] },
            "null-recipients" => fixture.Command with { Recipients = null! },
            "duplicate-recipients" => fixture.Command with { Recipients = [fixture.Command.Recipients[0], fixture.Command.Recipients[0]] },
            "invalid-batch" => fixture.Command with { BatchSize = 0 },
            "unknown-channel" => fixture.Command with { Channels = ["unknown"] },
            "null-channels" => fixture.Command with { Channels = null! },
            "missing-address" => fixture.Command with { Recipients = [new BulkNotificationRecipient { UserId = Guid.NewGuid() }] },
            "scheduled" => fixture.Command with { ScheduledAt = DateTimeOffset.UtcNow.AddHours(1) },
            "missing-requester" => fixture.Command with { RequestedBy = Guid.Empty },
            "missing-template" => fixture.Command with { TemplateKey = "" },
            "missing-sms-number" => fixture.Command with { Channels = [NotificationChannelNames.Sms] },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        using (new RequestCultureScope("ja-JP"))
            await fixture.Consume(command);
        var result = Assert.Single(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.NotNull(result.ErrorMessage);
        Assert.DoesNotMatch(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]", result.ErrorMessage);
        using (new RequestCultureScope("ja-JP"))
            Assert.Equal(japanese, UserMessageLocalizer.Translate(result.ErrorMessage));
        using (new RequestCultureScope("en-US"))
            Assert.Equal(result.ErrorMessage, UserMessageLocalizer.Translate(result.ErrorMessage));
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFinalFailure);
        Assert.Equal(0, fixture.EmailSends);
        Assert.Equal(0, fixture.PushSends);
        Assert.Empty(fixture.Published.OfType<RequestJobCommand>());
        Assert.Empty(fixture.Published.OfType<BulkNotificationCompletedEvent>());
    }

    [Fact]
    public async Task Cancellation_DoesNotCompleteOrFailTheItem()
    {
        var fixture = new Fixture { CancelDelivery = true };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Consume(fixture.Command));
        Assert.Empty(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.Empty(fixture.Published.OfType<BulkNotificationCompletedEvent>());
    }

    private sealed class Fixture
    {
        public SendBulkNotificationCommand Command { get; } = new()
        {
            ParentJobId = Guid.NewGuid(), ParentJobItemId = Guid.NewGuid(), RequestedBy = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(), TemplateKey = "announcement", Title = "Announcement", Message = "Hello",
            Channels = [NotificationChannelNames.Email, NotificationChannelNames.Push],
            Recipients = [new BulkNotificationRecipient { UserId = Guid.NewGuid(), Email = "test@example.com" }]
        };
        public List<object> Published { get; } = [];
        private HashSet<(string, string)> Receipts { get; } = [];
        public int EmailSends { get; private set; }
        public int PushSends { get; private set; }
        public bool PushSucceeds { get; set; } = true;
        public bool CancelDelivery { get; init; }

        public Task Consume(SendBulkNotificationCommand command)
        {
            var context = TestProxy.Create<ConsumeContext<SendBulkNotificationCommand>>((method, args) => method.Name switch
            {
                "get_Message" => command,
                "get_CancellationToken" => CancellationToken.None,
                "Publish" => Capture(args![0]!),
                _ => throw new NotSupportedException(method.Name)
            });
            var publisher = TestProxy.Create<IPublishEndpoint>((method, args) => Capture(args![0]!));
            var idempotency = TestProxy.Create<IIdempotencyService>((method, args) => method.Name switch
            {
                nameof(IIdempotencyService.IsProcessedAsync) => Task.FromResult(Receipts.Contains(((string)args![0]!, (string)args[1]!))),
                nameof(IIdempotencyService.MarkAsProcessedAsync) => Mark((string)args![0]!, (string)args[1]!),
                _ => throw new NotSupportedException(method.Name)
            });
            var templates = TestProxy.Create<ITemplateRepository>((_, _) => Task.FromResult<NotificationTemplate?>(null));
            var records = TestProxy.Create<INotificationRecordRepository>((_, _) => Task.CompletedTask);
            var unitOfWork = TestProxy.Create<IUnitOfWork>((_, _) => Task.FromResult(1));
            var email = TestProxy.Create<IEmailSender>((_, _) =>
            {
                EmailSends++;
                return CancelDelivery ? Task.FromException<EmailSendResult>(new OperationCanceledException()) :
                    Task.FromResult(new EmailSendResult(true));
            });
            var push = TestProxy.Create<IPushSender>((_, _) =>
            {
                PushSends++;
                return Task.FromResult(new PushSendResult(PushSucceeds));
            });
            return new SendBulkNotificationConsumer(idempotency, templates, records, unitOfWork, email, null!, push,
                null!, publisher, NullLogger<SendBulkNotificationConsumer>.Instance).Consume(context);
        }

        private Task Capture(object message) { Published.Add(message); return Task.CompletedTask; }
        private Task Mark(string key, string channel) { Receipts.Add((key, channel)); return Task.CompletedTask; }
    }
}
