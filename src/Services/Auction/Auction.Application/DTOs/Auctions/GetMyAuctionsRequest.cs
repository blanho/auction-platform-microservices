using BuildingBlocks.Application.Constants;

namespace Auctions.Application.DTOs.Auctions;

public class GetMyAuctionsRequest
{
    public string? Status { get; set; }

    public string? SearchTerm { get; set; }

    public int Page { get; set; } = PaginationDefaults.DefaultPage;

    public int PageSize { get; set; } = PaginationDefaults.DefaultPageSize;

    public string? OrderBy { get; set; }

    public bool Descending { get; set; } = false;
}

