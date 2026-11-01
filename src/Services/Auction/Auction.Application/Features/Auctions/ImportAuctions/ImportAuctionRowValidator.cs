using BuildingBlocks.Domain.Constants;

namespace Auctions.Application.Features.Auctions.ImportAuctions;

public static class ImportAuctionRowValidator
{
    private const int EarliestManufacturingYear = 1900;

    public static List<ImportRowError> Validate(ImportAuctionRow row, int rowNumber, DateTimeOffset now)
    {
        var errors = new List<ImportRowError>();

        if (string.IsNullOrWhiteSpace(row.Title))
        {
            errors.Add(new ImportRowError(rowNumber, nameof(row.Title), "Title is required."));
        }
        else if (row.Title.Length > ValidationConstants.StringLength.Medium)
        {
            errors.Add(new ImportRowError(rowNumber, nameof(row.Title),
                $"Title must not exceed {ValidationConstants.StringLength.Medium} characters."));
        }

        if (string.IsNullOrWhiteSpace(row.Description))
        {
            errors.Add(new ImportRowError(rowNumber, nameof(row.Description), "Description is required."));
        }

        if (row.ReservePrice < 0)
        {
            errors.Add(new ImportRowError(rowNumber, nameof(row.ReservePrice), "Reserve price must be non-negative."));
        }

        if (row.BuyNowPrice.HasValue && row.BuyNowPrice.Value <= row.ReservePrice)
        {
            errors.Add(new ImportRowError(rowNumber, nameof(row.BuyNowPrice), "Buy now price must exceed reserve price."));
        }

        if (row.AuctionEnd <= now)
        {
            errors.Add(new ImportRowError(rowNumber, nameof(row.AuctionEnd), "Auction end date must be in the future."));
        }

        var latestManufacturingYear = now.UtcDateTime.Year + 1;
        if (row.YearManufactured.HasValue
            && (row.YearManufactured < EarliestManufacturingYear || row.YearManufactured > latestManufacturingYear))
        {
            errors.Add(new ImportRowError(rowNumber, nameof(row.YearManufactured),
                $"Year must be between {EarliestManufacturingYear} and {latestManufacturingYear}."));
        }

        return errors;
    }
}
