using Common.Core.Constants;

namespace AuctionService.Application.DTOs.Requests;

public class GetMyAuctionsRequest
{
    public string? Status { get; set; }

    public string? SearchTerm { get; set; }

    public int PageNumber { get; set; } = PaginationDefaults.DefaultPage;

    public int PageSize { get; set; } = PaginationDefaults.DefaultPageSize;

    public string? OrderBy { get; set; }

    public bool Descending { get; set; } = false;
}
