using BuildingBlocks.Application.Constants;

namespace Auctions.Application.DTOs.Auctions;

public class GetMyAuctionsRequest
{
    public string? Status { get; set; }

    public string? SearchTerm { get; set; }

    public int? Page { get; set; }

    public int? PageSize { get; set; }

    public string? OrderBy { get; set; }

    public bool? Descending { get; set; }
}

