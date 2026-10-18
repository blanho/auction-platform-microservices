using AutoMapper;
using BuildingBlocks.Application.Localization;
using Jobs.Application.DTOs;
using Jobs.Application.Mappings;
using Jobs.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jobs.Application.Tests;

public class JobLocalizationTests
{
    [Fact]
    public void KeyedItemErrorsDoNotDependOnEnglishFallbackWording()
    {
        var mapper = new MapperConfiguration(config => config.AddProfile<JobMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();
        var job = Jobs.Domain.Entities.Job.Create(JobType.BulkNotification, "keyed-job", "{}", Guid.NewGuid(), 1);
        var item = job.AddItem("{}", 1);
        job.Start();
        item.MarkProcessing();
        const string code = "Notification.Bulk.BatchSizeOutOfRange";
        const string diagnostic = "Different English fallback wording";
        var arguments = new[] { "10", "500" };
        item.MarkFailed(diagnostic, terminal: true, errorCode: code, errorArguments: arguments);
        arguments[0] = "changed by caller";

        foreach (var language in new[] { "ja-JP", "en-US", "ja-JP" })
        {
            using var culture = new RequestCultureScope(language);
            var dto = mapper.Map<JobItemDto>(item);
            Assert.Equal(language == "ja-JP"
                ? "バッチサイズは10から500の間である必要があります。"
                : "BatchSize must be between 10 and 500.", dto.ErrorMessage);
            Assert.Equal(code, dto.ErrorCode);
            Assert.Equal(new[] { "10", "500" }, dto.ErrorArguments);
        }
        Assert.Equal(diagnostic, item.ErrorMessage);
        Assert.Equal(new[] { "10", "500" }, item.ErrorArguments);
        item.ResetForRetry();
        Assert.Null(item.ErrorCode);
        Assert.Empty(item.ErrorArguments);
        Assert.Null(item.ErrorMessage);
        item.MarkProcessing();
        item.MarkCompleted();
        Assert.Null(mapper.Map<JobItemDto>(item).ErrorMessage);
    }

    [Theory]
    [InlineData("Notification.Bulk.UnknownKey", false)]
    [InlineData("Notification.Bulk.BatchSizeOutOfRange", true)]
    public void UnknownKeysAndMissingArgumentsUseTheLegacyDiagnostic(string code, bool missingArguments)
    {
        using var culture = new RequestCultureScope("ja-JP");
        Assert.Equal("ファイルのアップロードが完了していません。",
            UserMessageLocalizer.Format(code, missingArguments ? ["10"] : [], "The uploaded file is incomplete."));
    }

    [Theory]
    [InlineData("Image processing failed after broker retries.", "再試行後も画像処理に失敗しました。")]
    [InlineData("File upload finalization failed after broker retries.", "再試行後もファイルのアップロード確定に失敗しました。")]
    [InlineData("Report generation failed after broker retries.", "再試行後もレポートの生成に失敗しました。")]
    [InlineData("Bulk notification failed after broker retries.", "再試行後も一括通知に失敗しました。")]
    [InlineData("Search reindexing failed after broker retries.", "再試行後も検索インデックスの再構築に失敗しました。")]
    [InlineData("Analytics aggregation failed after broker retries.", "再試行後も分析データの集計に失敗しました。")]
    [InlineData("The uploaded file is incomplete.", "ファイルのアップロードが完了していません。")]
    [InlineData("Image dimensions exceed the 16-megapixel limit.", "画像の画素数が上限の1,600万画素を超えています。")]
    [InlineData("Only confirmed uploads can be processed.", "確認済みのアップロードのみ処理できます。")]
    [InlineData("Provide a requester and 1–500 distinct, nonempty auction IDs.", "リクエスト者と、重複のない有効なオークションIDを1〜500件指定してください。")]
    [InlineData("Provide a requester and an inclusive range of 1–31 UTC dates, ending no later than today.", "リクエスト者と、今日以前に終了する1〜31日間のUTC日付範囲（開始日と終了日を含む）を指定してください。")]
    [InlineData("Delivery failed for 2 of 10 recipients.", "10人の宛先のうち2人への配信に失敗しました。")]
    [InlineData("Unsupported export format: Xml", "エクスポート形式「Xml」には対応していません。")]
    [InlineData("BatchSize must be between 10 and 500.", "バッチサイズは10から500の間である必要があります。")]
    public void ServiceJobDiagnosticsUseTheReadersLanguage(string diagnostic, string japanese)
    {
        var mapper = new MapperConfiguration(config => config.AddProfile<JobMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();
        var job = Jobs.Domain.Entities.Job.Create(JobType.ImageProcessing, "localized-job", "{}", Guid.NewGuid(), 1);
        var item = job.AddItem("{}", 1);
        job.Start();
        item.MarkProcessing();
        item.MarkFailed(diagnostic, terminal: true);
        job.Fail(diagnostic);

        foreach (var culture in new[] { "ja-JP", "en-US", "ja-JP" })
        {
            using var scope = new RequestCultureScope(culture);
            var expected = culture == "ja-JP" ? japanese : diagnostic;
            Assert.Equal(expected, mapper.Map<JobDto>(job).ErrorMessage);
            Assert.Equal(expected, mapper.Map<JobItemDto>(item).ErrorMessage);
        }
        Assert.Equal(diagnostic, job.ErrorMessage);
        Assert.Equal(diagnostic, item.ErrorMessage);
    }

    [Fact]
    public void MappingLocalizesEachReadWithoutChangingStoredDiagnostics()
    {
        var mapper = new MapperConfiguration(config => config.AddProfile<JobMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();
        var job = Jobs.Domain.Entities.Job.Create(JobType.AuctionImport, "localization", "{}", Guid.NewGuid(), 1);
        var item = job.AddItem("{}", 1);
        job.Start();
        item.MarkProcessing();
        const string failure = "Auction workflow failed after broker retries.";
        item.MarkFailed(failure, terminal: true);
        job.Fail(failure);

        using (new RequestCultureScope("ja-JP"))
        {
            Assert.Equal("再試行後もオークション処理に失敗しました。", mapper.Map<JobDto>(job).ErrorMessage);
            Assert.Equal("再試行後もオークション処理に失敗しました。", mapper.Map<JobItemDto>(item).ErrorMessage);
        }
        using (new RequestCultureScope("en-US"))
            Assert.Equal(failure, mapper.Map<JobDto>(job).ErrorMessage);
        Assert.Equal(failure, job.ErrorMessage);
        Assert.Equal(failure, item.ErrorMessage);
    }
}
