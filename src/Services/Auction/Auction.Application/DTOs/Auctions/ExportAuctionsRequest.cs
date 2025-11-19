namespace Auctions.Application.DTOs.Auctions;

public class ExportAuctionsRequest
{
    public string? Status { get; set; }

    public string? Seller { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public string Format { get; set; } = "json";
}

