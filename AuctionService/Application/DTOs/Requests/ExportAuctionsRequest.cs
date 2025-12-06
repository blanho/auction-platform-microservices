namespace AuctionService.Application.DTOs.Requests;

/// <summary>
/// Request DTO for exporting auctions with filtering options.
/// </summary>
public class ExportAuctionsRequest
{
    /// <summary>
    /// Filter by auction status (Live, ReserveNotMet, Finished, etc.)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by seller username
    /// </summary>
    public string? Seller { get; set; }

    /// <summary>
    /// Filter auctions ending after this date
    /// </summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Filter auctions ending before this date
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// Export format: json, csv, or excel. Default: json
    /// </summary>
    public string Format { get; set; } = "json";
}
