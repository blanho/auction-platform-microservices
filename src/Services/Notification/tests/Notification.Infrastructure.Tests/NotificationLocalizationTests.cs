using BuildingBlocks.Application.Localization;
using BuildingBlocks.Web.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Localization;
using Notification.Application.Resources;
using Xunit;

namespace Notification.Infrastructure.Tests;

public class NotificationLocalizationTests
{
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
