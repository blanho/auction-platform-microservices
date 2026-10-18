using BuildingBlocks.Application.Localization;
using BuildingBlocks.Web.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Localization;
using Notification.Application.Resources;
using Notification.Application.Interfaces;
using Notification.Application.Services;
using Notification.Domain.Entities;
using TestSupport;
using Xunit;

namespace Notification.Infrastructure.Tests;

public class NotificationLocalizationTests
{
    [Theory]
    [InlineData("Bulk notification failed after broker retries.", "再試行後も一括通知に失敗しました。")]
    [InlineData("Recipient email is required", "宛先メールアドレスは必須です。")]
    [InlineData("Phone number is required", "電話番号は必須です。")]
    [InlineData("Title or body is required", "タイトルまたは本文は必須です。")]
    [InlineData("Twilio not configured", "SMS配信が設定されていません。")]
    [InlineData("Invalid phone number format", "電話番号の形式が無効です。")]
    [InlineData("No device tokens provided", "デバイストークンが指定されていません。")]
    [InlineData("All 5 sends failed", "すべての5件の配信に失敗しました。")]
    [InlineData("SendGrid error: BadRequest - provider-specific diagnostic", "メールの配信に失敗しました（ステータス: BadRequest）。")]
    [InlineData("Twilio error 21211: provider-specific diagnostic", "SMSの配信に失敗しました（エラーコード: 21211）。")]
    [InlineData("Provider connection failed at internal-host:123", "操作を完了できませんでした。もう一度お試しいただくか、サポートにお問い合わせください。")]
    [InlineData(null, null)]
    public async Task DeliveryRecordErrorsLocalizeWithoutChangingStoredContent(string? diagnostic, string? japanese)
    {
        var record = NotificationRecord.Create(Guid.NewGuid(), "announcement", "Email", "User-written subject", "reader@example.test");
        if (diagnostic is not null) record.MarkAsFailed(diagnostic);
        var repository = TestProxy.Create<INotificationRecordRepository>((method, _) => method.Name switch
        {
            nameof(INotificationRecordRepository.GetByIdAsync) => Task.FromResult<NotificationRecord?>(record),
            _ => throw new NotSupportedException(method.Name)
        });
        var service = new NotificationRecordService(repository);
        foreach (var language in new[] { "ja-JP", "en-US", "ja-JP" })
        {
            using var culture = new RequestCultureScope(language);
            var result = await service.GetByIdAsync(record.Id);
            Assert.NotNull(result);
            Assert.Equal(language == "ja-JP" ? japanese : diagnostic, result.ErrorMessage);
            Assert.Equal(record.Subject, result.Subject);
            Assert.Equal(record.Channel, result.Channel);
            Assert.Equal(record.Status.ToString(), result.Status);
            Assert.Equal(record.Recipient, result.Recipient);
        }
        Assert.Equal(diagnostic, record.ErrorMessage);
    }

    [Fact]
    public void JobNotificationCanBeReadInBothLanguagesWithoutChangingStoredArguments()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppLocalization<NotificationResources>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        const string diagnostic = "Auction workflow failed after broker retries.";
        (string Title, string Message, string Data) stored;
        using (new RequestCultureScope("ja-JP"))
        {
            stored = NotificationLocalizationMetadata.ResolveForStorage(new CreateNotificationDto
            {
                Data = "{\"jobId\":\"job-123\"}",
                LocalizedText = new LocalizedNotificationText(NotificationMessageKeys.JobFailedTitle,
                    NotificationMessageKeys.JobFailedMessage, "AuctionImport", diagnostic)
            }, localizer);
            Assert.Equal("ジョブ「オークションインポート」に失敗しました: 再試行後もオークション処理に失敗しました。", stored.Message);
        }

        using (new RequestCultureScope("en-US"))
        {
            var english = NotificationLocalizationMetadata.ResolveForResponse(stored.Title, stored.Message, stored.Data, localizer);
            Assert.Equal($"Job 'AuctionImport' has failed: {diagnostic}", english.Message);
            Assert.Equal("{\"jobId\":\"job-123\"}", english.Data);
        }

        using (new RequestCultureScope("ja-JP"))
        {
            var japanese = NotificationLocalizationMetadata.ResolveForResponse(stored.Title, stored.Message, stored.Data, localizer);
            Assert.Equal(stored.Message, japanese.Message);
        }
        Assert.Contains(diagnostic, stored.Data);
        Assert.Contains("AuctionImport", stored.Data);
    }
}
