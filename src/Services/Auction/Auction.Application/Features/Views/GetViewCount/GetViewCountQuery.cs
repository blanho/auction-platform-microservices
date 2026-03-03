using Auctions.Application.DTOs.Views;

namespace Auctions.Application.Features.Views.GetViewCount;

public record GetViewCountQuery(Guid AuctionId) : IQuery<ViewCountDto>;
