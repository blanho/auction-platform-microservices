using BuildingBlocks.Application.Localization;
using BuildingBlocks.Application.Paging;
using BuildingBlocks.Web.Helpers;
using Xunit;

namespace BuildingBlocks.Web.Tests;

public sealed class PagingLocalizationTests
{
    [Fact]
    public void SortValidation_UsesJapaneseFieldLabels()
    {
        using var culture = new RequestCultureScope("ja-JP");
        var errors = new QueryParametersValidator().Validate(new QueryParameters
        {
            Sorts = [null!, new SortDescriptor { Field = "" }]
        }).Errors;
        var localized = errors.Select(error => ValidationErrorLocalizer.LocalizeMessage(error.ErrorMessage, null)).ToList();
        Assert.Contains(localized, message => message.Contains("並び順") && !message.Contains("Sort"));
        Assert.Contains(localized, message => message.Contains("並び替え項目") && !message.Contains("SortBy"));
    }
}
