#nullable enable
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;

namespace Auctions.Api.Extensions;

public static class EndpointAuthorizationExtensions
{

    public static async Task<AuthorizedAuctionResult> GetAuthorizedAuctionAsync(
        this IAuctionRepository repository,
        HttpContext httpContext,
        Guid auctionId,
        string permission,
        CancellationToken cancellationToken,
        bool requireOwnership = true)
    {
        var auction = await repository.GetByIdAsync(auctionId, cancellationToken);

        if (auction == null)
        {
            return AuthorizedAuctionResult.NotFound(auctionId);
        }

        var user = httpContext.User;

        if (user.IsAdmin())
        {
            return AuthorizedAuctionResult.Success(auction);
        }

        if (!user.HasPermission(permission))
        {
            return AuthorizedAuctionResult.Forbidden();
        }

        if (requireOwnership && !user.IsOwner(auction.SellerId))
        {
            return AuthorizedAuctionResult.Forbidden();
        }

        return AuthorizedAuctionResult.Success(auction);
    }
}

public readonly struct AuthorizedAuctionResult
{
    public Auction? Auction { get; }
    public IResult? ErrorResult { get; }
    public bool IsSuccess => Auction != null && ErrorResult == null;

    private AuthorizedAuctionResult(Auction? auction, IResult? errorResult)
    {
        Auction = auction;
        ErrorResult = errorResult;
    }

    public static AuthorizedAuctionResult Success(Auction auction)
        => new(auction, null);

    public static AuthorizedAuctionResult NotFound(Guid auctionId)
        => new(null, Results.NotFound(ProblemDetailsHelper.FromError(
            Error.Create("Auction.NotFound", $"Auction with ID {auctionId} was not found"))));

    public static AuthorizedAuctionResult Forbidden()
        => new(null, Results.Forbid());

    public void Deconstruct(out Auction? auction, out IResult? errorResult)
    {
        auction = Auction;
        errorResult = ErrorResult;
    }
}
