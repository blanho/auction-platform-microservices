using Auctions.Application.DTOs;
namespace Auctions.Application.Commands.AddSellerResponse;

public record AddSellerResponseCommand(
    Guid ReviewId,
    string SellerUsername,
    string Response
) : ICommand<ReviewDto>;

