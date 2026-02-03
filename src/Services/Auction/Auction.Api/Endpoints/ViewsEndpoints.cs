using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auctions.Application.Interfaces;
using Auctions.Infrastructure.Persistence;
using Auctions.Application.DTOs.Views;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Web.Authorization;

namespace Auctions.Api.Endpoints;

public static class ViewsEndpoints
{
    public static IEndpointRouteBuilder MapViewsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auctions/{auctionId:guid}/views")
            .WithTags("Auction Views");

        group.MapPost("/", RecordView)
            .WithName("RecordAuctionView")
            .RequireAuthorization()
            .Produces<RecordViewResponseDto>();

        group.MapGet("/count", GetViewCount)
            .WithName("GetAuctionViewCount")
            .Produces<ViewCountDto>();

        return app;
    }

    private static async Task<IResult> RecordView(
        [FromRoute] Guid auctionId,
        [FromServices] IAuctionViewRepository repository,
        [FromServices] IUnitOfWork unitOfWork,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        await repository.RecordViewAsync(auctionId, userId, ipAddress, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(new RecordViewResponseDto { Success = true });
    }

    private static async Task<IResult> GetViewCount(
        [FromRoute] Guid auctionId,
        [FromServices] IAuctionViewRepository repository,
        CancellationToken cancellationToken)
    {
        var count = await repository.GetViewCountForAuctionAsync(auctionId, cancellationToken);
        return Results.Ok(new ViewCountDto { AuctionId = auctionId, ViewCount = count });
    }
}
