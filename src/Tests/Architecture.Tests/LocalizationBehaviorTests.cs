using System.Globalization;
using Auctions.Application.Errors;
using Auctions.Application.Resources;
using BuildingBlocks.Application.Abstractions;
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
        Assert.Equal("Titleフィールドは必須です。", localizedErrors["Title"].Single());
        Assert.Equal(
            "Auction end dateフィールドは現在から30日以内でなければなりません。",
            localizedErrors["AuctionEnd"].Single());
    }
}
