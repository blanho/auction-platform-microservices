#nullable enable
using Auctions.Application.Features.Reviews.AddSellerResponse;
using Auctions.Application.Features.Reviews.CreateReview;
using Auctions.Application.DTOs;
using Auctions.Application.Features.Reviews.GetReviewById;
using Auctions.Application.Features.Reviews.GetReviewsByUser;
using Auctions.Application.Features.Reviews.GetReviewsForAuction;
using Auctions.Application.Features.Reviews.GetReviewsForUser;
using Auctions.Application.Features.Reviews.GetUserRatingSummary;
using BuildingBlocks.Web.Authorization;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Reviews;

public class ReviewEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reviews")
            .WithTags("Reviews")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetReviewById)
            .WithName("GetReviewById")
            .Produces<ReviewDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/auction/{auctionId:guid}", GetReviewsForAuction)
            .WithName("GetReviewsForAuction")
            .Produces<List<ReviewDto>>(StatusCodes.Status200OK);

        group.MapGet("/user/{username}", GetReviewsForUser)
            .WithName("GetReviewsForUser")
            .Produces<List<ReviewDto>>(StatusCodes.Status200OK);

        group.MapGet("/by/{username}", GetReviewsByUser)
            .WithName("GetReviewsByUser")
            .Produces<List<ReviewDto>>(StatusCodes.Status200OK);

        group.MapGet("/user/{username}/summary", GetUserRatingSummary)
            .WithName("GetUserRatingSummary")
            .Produces<UserRatingSummaryDto>(StatusCodes.Status200OK);

        group.MapPost("/", CreateReview)
            .WithName("CreateReview")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reviews.Create))
            .Produces<ReviewDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/response", AddSellerResponse)
            .WithName("AddSellerResponse")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reviews.Edit))
            .Produces<ReviewDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetReviewById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetReviewByIdQuery(id), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetReviewsForAuction(
        Guid auctionId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetReviewsForAuctionQuery(auctionId), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetReviewsForUser(
        string username,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetReviewsForUserQuery(username), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetReviewsByUser(
        string username,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetReviewsByUserQuery(username), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetUserRatingSummary(
        string username,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserRatingSummaryQuery(username), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> CreateReview(
        CreateReviewDto dto,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var reviewerId = UserHelper.GetRequiredUserId(httpContext.User);
        var reviewerUsername = UserHelper.GetUsername(httpContext.User);

        var command = new CreateReviewCommand(
            dto.AuctionId,
            dto.OrderId,
            reviewerId,
            reviewerUsername,
            dto.ReviewedUserId,
            dto.ReviewedUsername,
            dto.Rating,
            dto.Title,
            dto.Comment);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetReviewById", new { id = result.Value!.Id }, result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> AddSellerResponse(
        Guid id,
        AddSellerResponseDto dto,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);

        var command = new AddSellerResponseCommand(id, username, dto.Response);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}

