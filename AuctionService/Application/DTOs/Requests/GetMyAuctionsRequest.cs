namespace AuctionService.Application.DTOs.Requests;

public class GetMyAuctionsRequest
{
    public string? Status { get; set; }

    public string? SearchTerm { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? OrderBy { get; set; }

    public bool Descending { get; set; } = false;
}
