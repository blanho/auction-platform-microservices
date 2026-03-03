#nullable enable
using Auctions.Application.Features.Views.RecordView;
using Auctions.Application.DTOs.Views;
using Auctions.Application.Features.Views.GetViewCount;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using Carter;
using MediatR;
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
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetUserId(httpContext.User)?.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var command = new RecordViewCommand(auctionId, userId, ipAddress);
        var result = await mediator.Send(command, ct);

        return result.ToApiResult(_ => Results.Ok(new RecordViewResponseDto { Success = true }));
    }

    private static async Task<IResult> GetViewCount(
        [FromRoute] Guid auctionId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetViewCountQuery(auctionId);
        var result = await mediator.Send(query, ct);

        return result.ToOkResult();
    }
}
