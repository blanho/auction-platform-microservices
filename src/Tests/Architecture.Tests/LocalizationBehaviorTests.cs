using System.Globalization;
using Auctions.Application.Errors;
using Auctions.Application.Features.Auctions.UpdateAuction;
using Auctions.Application.Resources;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Web.Constants;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Architecture.Tests;

public sealed class LocalizationBehaviorTests
{
    [Theory]
    [InlineData("The uploaded file is incomplete.", "ファイルのアップロードが完了していません。")]
    [InlineData("Provider failed on internal-host", "操作を完了できませんでした。もう一度お試しいただくか、サポートにお問い合わせください。")]
    public void NestedDiagnosticArgumentsFollowTheRequestLanguage(string diagnostic, string japanese)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppLocalization<AuctionResources>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var error = Assert.IsType<LocalizableError>(AuctionErrors.BuyNow.Failed(diagnostic));
        var originalMessage = error.Message;

        foreach (var language in new[] { "ja-JP", "en-US", "ja-JP" })
        {
            using var culture = new RequestCultureScope(language);
            var problem = ProblemDetailsHelper.FromError(error, localizer);
            Assert.Equal(language == "ja-JP"
                ? $"即購入の処理に失敗しました: {japanese}"
                : $"Failed to process Buy Now: {diagnostic}", problem.Detail);
            Assert.Equal("BuyNow.Failed", problem.Title);
        }
        Assert.Equal(diagnostic, Assert.Single(error.Parameters));
        Assert.Equal(originalMessage, error.Message);
    }

    [Fact]
    public void NonDiagnosticArgumentsRetainUserProvidedValues()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppLocalization<AuctionResources>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        const string slug = "The uploaded file is incomplete.";
        using var culture = new RequestCultureScope("ja-JP");
        var problem = ProblemDetailsHelper.FromError(AuctionErrors.Brand.SlugExists(slug), localizer);
        Assert.Equal($"スラッグ '{slug}' のブランドはすでに存在します。", problem.Detail);
    }

    [Fact]
    public void DiagnosticTranslationPreservesOtherTemplateArguments()
    {
        const string name = "The uploaded file is incomplete.";
        const string diagnostic = "Only confirmed uploads can be processed.";
        var id = Guid.NewGuid();
        var error = LocalizableError.WithDiagnostic("Test.ContextError", "Original diagnostic",
            17, name, id, diagnostic);
        var original = error.Parameters.ToArray();
        var localizer = new StubLocalizer("Test.ContextError", "{0}件、{1}（{2}）: {3}");
        using var culture = new RequestCultureScope("ja-JP");
        var problem = ProblemDetailsHelper.FromError(error, localizer);
        Assert.Equal($"17件、{name}（{id}）: 確認済みのアップロードのみ処理できます。", problem.Detail);
        Assert.Equal(original, error.Parameters);
        Assert.Equal("Original diagnostic", error.Message);
    }

    [Fact]
    public void MissingServiceValidationResourcesUseTheSharedMessageTranslation()
    {
        using var culture = new RequestCultureScope("ja-JP");
        Assert.Equal("タイトルは必須です。",
            ValidationErrorLocalizer.LocalizeMessage("Title is required.", new StubLocalizer()));
        Assert.Equal("終了日時は現在から30日以内である必要があります。",
            ValidationErrorLocalizer.LocalizeMessage("Auction end date must not exceed 30 days from now.", null));
    }

    [Fact]
    public void PastAuctionEndValidationRemainsActionableInBothLanguages()
    {
        var command = new UpdateAuctionCommand(Guid.NewGuid(), null, null, null, null, null,
            null, null, null, null, null, DateTimeOffset.UtcNow.AddDays(-1), Guid.NewGuid());
        var result = new UpdateAuctionCommandValidator().Validate(command);
        var diagnostic = Assert.Single(result.Errors).ErrorMessage;
        Assert.Equal("Auction end date must be in the future.", diagnostic);
        using var culture = new RequestCultureScope("ja-JP");
        Assert.Equal("オークション終了日時は未来の日時を指定してください。", UserMessageLocalizer.Translate(diagnostic));
    }

    private sealed class StubLocalizer(string? resourceKey = null, string? template = null) : ILocalizationService
    {
        public string CurrentCulture => CultureInfo.CurrentUICulture.Name;
        public string this[string key] => GetString(key);
        public string this[string key, params object[] arguments] => GetString(key, arguments);
        public string GetString(string key) => GetLocalizedString(key).Value;
        public string GetString(string key, params object[] arguments) => GetLocalizedString(key, arguments).Value;
        public LocalizedString GetLocalizedString(string key) => key == resourceKey
            ? new(key, template!) : new(key, key, true);
        public LocalizedString GetLocalizedString(string key, params object[] arguments)
        {
            var resource = GetLocalizedString(key);
            return resource.ResourceNotFound ? resource : resource with { Value = string.Format(resource.Value, arguments) };
        }
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures = true) => [];
    }

    [Fact]
    public async Task ProblemDetails_UsesTheRequestCulture()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppLocalization<AuctionResources>();

        await using var provider = services.BuildServiceProvider();
        var application = new ApplicationBuilder(provider);
        string? localizedDetail = null;

        application.UseAppLocalization();
        application.Run(context =>
        {
            localizedDetail = ProblemDetailsHelper.FromError(AuctionErrors.Auction.Forbidden).Detail;
            return Task.CompletedTask;
        });

        await using var scope = provider.CreateAsyncScope();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        context.Request.Headers.AcceptLanguage = "ja-JP";

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            await application.Build()(context);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }

        Assert.Equal("このオークションを変更する権限がありません。", localizedDetail);
        Assert.Equal("ja-JP", context.Response.Headers.ContentLanguage);
    }

    [Fact]
    public async Task ValidationProblemDetails_LocalizesSharedValidationMessages()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppLocalization<AuctionResources>();

        await using var provider = services.BuildServiceProvider();
        var application = new ApplicationBuilder(provider);
        ProblemDetails? problemDetails = null;

        application.UseAppLocalization();
        application.Run(context =>
        {
            var error = ValidationError.WithErrors(new Dictionary<string, string[]>
            {
                ["Title"] = ["Title is required"],
                ["AuctionEnd"] = ["Auction end date must not exceed 30 days from now."]
            });
            problemDetails = ProblemDetailsHelper.FromError(error);
            return Task.CompletedTask;
        });

        await using var scope = provider.CreateAsyncScope();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        context.Request.Headers.AcceptLanguage = "ja-JP";

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            await application.Build()(context);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }

        var localizedErrors = Assert.IsType<Dictionary<string, string[]>>(
            problemDetails!.Extensions[ProblemDetailsExtensionKeys.Errors]);
        Assert.Equal("1つ以上の検証エラーが発生しました。", problemDetails.Detail);
        Assert.Equal("タイトルフィールドは必須です。", localizedErrors["Title"].Single());
        Assert.Equal(
            "終了日時フィールドは現在から30日以内でなければなりません。",
            localizedErrors["AuctionEnd"].Single());
    }
}
