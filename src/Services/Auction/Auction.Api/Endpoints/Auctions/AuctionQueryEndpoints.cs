#nullable enable
using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Queries.GetAuctionById;
using Auctions.Application.Queries.GetAuctionsByIds;
using Auctions.Application.Queries.GetAuctions;
using Auctions.Application.Queries.GetMyAuctions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Auctions;

public class AuctionQueryEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auctions")
            .WithOpenApi();

        group.MapGet("/", GetAuctions)
            .WithName("GetAuctions")
            .Produces<PaginatedResult<AuctionDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/featured", GetFeaturedAuctions)
            .WithName("GetFeaturedAuctions")
            .Produces<PaginatedResult<AuctionDto>>(StatusCodes.Status200OK);

        group.MapGet("/my", GetMyAuctions)
            .WithName("GetMyAuctions")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.View))
            .Produces<PaginatedResult<AuctionDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetAuctionById)
            .WithName("GetAuctionById")
            .Produces<AuctionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/batch", GetAuctionsByIds)
            .WithName("GetAuctionsByIds")
            .Produces<List<AuctionDto>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAuctions(
        [AsParameters] GetAuctionsRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetAuctionsQuery(
            request.Status, request.Seller, request.Winner, request.SearchTerm,
            request.Category, request.IsFeatured,
            request.Page, request.PageSize, request.OrderBy, request.Descending);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetFeaturedAuctions(
        int page,
        int pageSize,
        IMediator mediator,
        CancellationToken ct)
    {
        var validPage = page > 0 ? page : 1;
        var validPageSize = pageSize > 0 && pageSize <= 50 ? pageSize : 8;

        var query = new GetAuctionsQuery(
            null, null, null, null,
            null, true,
            validPage, validPageSize, "auctionEnd", false);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetMyAuctions(
        [AsParameters] GetMyAuctionsRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);

        var query = new GetMyAuctionsQuery(
            username,
            request.Status,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            request.OrderBy,
            request.Descending);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetAuctionById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetAuctionByIdQuery(id);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetAuctionsByIds(
        List<Guid> ids,
        IMediator mediator,
        CancellationToken ct)
    {
        if (ids == null || ids.Count == 0)
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Validation.Empty", "At least one auction ID is required")));
        }

        if (ids.Count > 100)
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Validation.TooMany", "Maximum 100 auction IDs allowed per request")));
        }

        var query = new GetAuctionsByIdsQuery(ids.Distinct().ToList());
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}

