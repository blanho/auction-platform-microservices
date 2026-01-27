using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Reviews.AddSellerResponse;

public record AddSellerResponseCommand(
    Guid ReviewId,
    string SellerUsername,
    string Response
) : ICommand<ReviewDto>;
