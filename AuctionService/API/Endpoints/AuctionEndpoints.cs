#nullable enable
using Asp.Versioning;
using AuctionService.Application.Commands.ActivateAuction;
using AuctionService.Application.Commands.CreateAuction;
using AuctionService.Application.Commands.DeactivateAuction;
using AuctionService.Application.Commands.DeleteAuction;
using AuctionService.Application.Commands.UpdateAuction;
using AuctionService.Application.DTOs;
using AuctionService.Application.DTOs.Requests;
using AuctionService.Application.Queries.GetAuctionById;
using AuctionService.Application.Queries.GetAuctionsByIds;
using AuctionService.Application.Queries.GetAuctions;
using AuctionService.Application.Queries.GetMyAuctions;
using AuctionService.Application.Queries.GetSellerAnalytics;
using AuctionService.Application.Queries.GetQuickStats;
using AuctionService.Application.Queries.GetTrendingSearches;
using AuctionService.Application.Queries.GetUserDashboardStats;
using Carter;
using Common.Core.Helpers;
using Common.Utilities.Constants;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Endpoints;

public class AuctionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auctions")
            .WithOpenApi();

        group.MapGet("/", GetAuctions)
            .WithName("GetAuctions")
            .Produces<PagedResult<AuctionDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/featured", GetFeaturedAuctions)
            .WithName("GetFeaturedAuctions")
            .Produces<PagedResult<AuctionDto>>(StatusCodes.Status200OK);

        group.MapGet("/my", GetMyAuctions)
            .WithName("GetMyAuctions")
            .RequireAuthorization("AuctionScope")
            .Produces<PagedResult<AuctionDto>>(StatusCodes.Status200OK);

        group.MapGet("/dashboard/stats", GetDashboardStats)
            .WithName("GetDashboardStats")
            .RequireAuthorization("AuctionScope")
            .Produces<UserDashboardStatsDto>(StatusCodes.Status200OK);

        group.MapGet("/analytics/seller", GetSellerAnalytics)
            .WithName("GetSellerAnalytics")
            .RequireAuthorization("AuctionScope")
            .Produces<SellerAnalyticsDto>(StatusCodes.Status200OK);

        group.MapGet("/analytics/quick-stats", GetQuickStats)
            .WithName("GetQuickStats")
            .AllowAnonymous()
            .Produces<QuickStatsDto>(StatusCodes.Status200OK);

        group.MapGet("/analytics/trending-searches", GetTrendingSearches)
            .WithName("GetTrendingSearches")
            .AllowAnonymous()
            .Produces<List<TrendingSearchDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetAuctionById)
            .WithName("GetAuctionById")
            .Produces<AuctionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/batch", GetAuctionsByIds)
            .WithName("GetAuctionsByIds")
            .Produces<List<AuctionDto>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateAuction)
            .WithName("CreateAuction")
            .RequireAuthorization("AuctionWrite")
            .Produces<AuctionDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateAuction)
            .WithName("UpdateAuction")
            .RequireAuthorization("AuctionOwner")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", DeleteAuction)
            .WithName("DeleteAuction")
            .RequireAuthorization("AuctionOwner")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/activate", ActivateAuction)
            .WithName("ActivateAuction")
            .RequireAuthorization("AuctionOwner")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", DeactivateAuction)
            .WithName("DeactivateAuction")
            .RequireAuthorization("AuctionOwner")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAuctions(
        [AsParameters] GetAuctionsRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetAuctionsQuery(
            request.Status, request.Seller, request.Winner, request.SearchTerm,
            request.Category, request.IsFeatured,
            request.PageNumber, request.PageSize, request.OrderBy, request.Descending);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetFeaturedAuctions(
        int pageSize,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetAuctionsQuery(
            null, null, null, null,
            null, true,
            1, pageSize > 0 ? pageSize : 8, "auctionEnd", false);

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
            request.PageNumber,
            request.PageSize,
            request.OrderBy,
            request.Descending);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetDashboardStats(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var query = new GetUserDashboardStatsQuery(username);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetSellerAnalytics(
        string? timeRange,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var query = new GetSellerAnalyticsQuery(username, timeRange ?? "30d");
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetQuickStats(
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetQuickStatsQuery();
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetTrendingSearches(
        int? limit,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetTrendingSearchesQuery(limit ?? 6);
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
        var query = new GetAuctionsByIdsQuery(ids);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> CreateAuction(
        CreateAuctionWithFileIdsDto dto,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var sellerId = UserHelper.GetRequiredUserId(httpContext.User);
        var sellerUsername = UserHelper.GetUsername(httpContext.User);

        var commandFiles = dto.Files?.Select(f => new CreateAuctionFileDto(
            f.FileId,
            f.FileType,
            f.DisplayOrder,
            f.IsPrimary
        )).ToList();

        var command = new CreateAuctionCommand(
            dto.Title,
            dto.Description,
            dto.Condition,
            dto.YearManufactured,
            dto.Attributes,
            dto.ReservePrice,
            dto.BuyNowPrice,
            dto.AuctionEnd,
            sellerId,
            sellerUsername,
            dto.Currency,
            commandFiles,
            dto.CategoryId,
            dto.IsFeatured);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetAuctionById", new { id = result.Value!.Id }, new { id = result.Value.Id })
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> UpdateAuction(
        Guid id,
        UpdateAuctionDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateAuctionCommand(
            id,
            dto.Title,
            dto.Description,
            dto.Condition,
            dto.YearManufactured,
            dto.Attributes);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> DeleteAuction(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeleteAuctionCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ActivateAuction(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new ActivateAuctionCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> DeactivateAuction(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeactivateAuctionCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }
}
