#nullable enable
using System.Security.Claims;
using Auctions.Application.DTOs.Views;
using Auctions.Application.Interfaces;
using BuildingBlocks.Infrastructure.Repository;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Views;

public class ViewsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions/{auctionId:guid}/views")
            .WithTags("Auction Views")
            .WithOpenApi();

        group.MapPost("/", RecordView)
            .WithName("RecordAuctionView")
            .RequireAuthorization()
            .Produces<RecordViewResponseDto>(StatusCodes.Status200OK);

        group.MapGet("/count", GetViewCount)
            .WithName("GetAuctionViewCount")
            .Produces<ViewCountDto>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RecordView(
        [FromRoute] Guid auctionId,
        IAuctionViewRepository repository,
        IUnitOfWork unitOfWork,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        await repository.RecordViewAsync(auctionId, userId, ipAddress, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Results.Ok(new RecordViewResponseDto { Success = true });
    }

    private static async Task<IResult> GetViewCount(
        [FromRoute] Guid auctionId,
        IAuctionViewRepository repository,
        CancellationToken ct)
    {
        var count = await repository.GetViewCountForAuctionAsync(auctionId, ct);
        return Results.Ok(new ViewCountDto { AuctionId = auctionId, ViewCount = count });
    }
}
