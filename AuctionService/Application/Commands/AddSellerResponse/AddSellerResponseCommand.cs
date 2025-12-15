using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.AddSellerResponse;

public record AddSellerResponseCommand(
    Guid ReviewId,
    string SellerUsername,
    string Response
) : ICommand<ReviewDto>;
