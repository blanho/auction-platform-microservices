using System.Globalization;
using System.Text.Json;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Web.Helpers;
using BuildingBlocks.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Architecture.Tests;

public class UserMessageLocalizationTests
{
    [Theory]
    [InlineData("Title is required.", "タイトルは必須です。")]
    [InlineData("Title must not exceed 200 characters.", "タイトルは200文字以内で入力してください。")]
    [InlineData("Year must be between 1900 and 2027.", "年は1900から2027の間である必要があります。")]
    [InlineData("TotalItems must match the number of job items.", "合計件数はジョブ項目数と一致する必要があります。")]
    [InlineData("Item sequence numbers must be unique.", "項目の連番は重複できません。")]
    [InlineData("Cannot transition from Live to Finished", "開催中から終了に変更することはできません。")]
    [InlineData("No item processor is registered for ImageProcessing.", "画像処理の項目処理はサポートされていません。")]
    public void JapaneseMessagesPreserveParameters(string message, string expected)
    {
        using var culture = new RequestCultureScope("ja-JP");
        Assert.Equal(expected, UserMessageLocalizer.Translate(message));
    }

    [Fact]
    public void UnknownDiagnosticsUseLocalizedFallbackWithoutChangingEnglish()
    {
        const string diagnostic = "Database failure on internal host";
        using (new RequestCultureScope("ja-JP"))
            Assert.Equal("操作を完了できませんでした。もう一度お試しいただくか、サポートにお問い合わせください。",
                UserMessageLocalizer.Translate(diagnostic));
        using (new RequestCultureScope("en-US"))
            Assert.Equal(diagnostic, UserMessageLocalizer.Translate(diagnostic));
        Assert.Null(UserMessageLocalizer.TranslateOptional(null));
    }

    [Fact]
    public async Task ConcurrentScopesRestoreCultureAndDoNotLeak()
    {
        var before = CultureInfo.CurrentUICulture.Name;
        var results = await Task.WhenAll(Render("ja-JP"), Render("en-US"), Render("unsupported"));
        Assert.Equal(new[] { "タイトルは必須です。", "Title is required.", "Title is required." }, results);
        Assert.Equal(before, CultureInfo.CurrentUICulture.Name);

        static async Task<string> Render(string culture)
        {
            using var scope = new RequestCultureScope(culture);
            await Task.Yield();
            return UserMessageLocalizer.Translate("Title is required.");
        }
    }

    [Fact]
    public async Task DomainExceptionDetailsAreLocalized()
    {
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        using var culture = new RequestCultureScope("ja-JP");
        var app = new ApplicationBuilder(services);
        app.UseAppExceptionHandling();
        app.Run(_ => throw new DomainInvariantException("Seller cannot buy their own auction"));
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        await app.Build()(context);
        context.Response.Body.Position = 0;
        using var result = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("出品者は自分の商品を購入できません。", result.RootElement.GetProperty("detail").GetString());
    }
}
