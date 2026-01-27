using System.Text;
using System.Text.RegularExpressions;

namespace Auctions.Application.Helpers;

public static partial class AuctionSlugHelper
{
    public static string GenerateAuctionSlug(string title, Guid auctionId)
    {
        var baseSlug = GenerateSlug(title);
        var shortId = auctionId.ToString("N")[..8];
        return $"{baseSlug}-{shortId}";
    }

    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var slug = text.ToLowerInvariant();
        slug = NonAlphanumericRegex().Replace(slug, "");
        slug = WhitespaceRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return slug;
    }

    public static string GenerateCategorySlug(string categoryName, string? parentSlug = null)
    {
        var slug = GenerateSlug(categoryName);
        return parentSlug != null ? $"{parentSlug}/{slug}" : slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}
