using Bidding.Domain.Constants;
using Bidding.Domain.Enums;

namespace Bidding.Application.DTOs;

public record BidHistoryFilterRequest(
    Guid? AuctionId,
    BidStatus? Status,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page = BidDefaults.DefaultPage,
    int PageSize = BidDefaults.DefaultPageSize);
