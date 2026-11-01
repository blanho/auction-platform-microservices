#nullable enable
using Auctions.Application.Features.Auctions.ActivateAuction;
using Auctions.Application.Features.Auctions.CancelAuction;
using Auctions.Application.Features.Auctions.CreateAuction;
using Auctions.Application.Features.Auctions.DeactivateAuction;
using Auctions.Application.Features.Auctions.DeleteAuction;
using Auctions.Application.Features.Auctions.ExtendAuction;
using Auctions.Application.Features.Auctions.UpdateAuction;
using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Auctions;

public class AuctionCrudEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auctions")
            .WithOpenApi();

        group.MapPost("/", CreateAuction)
            .WithName("CreateAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Create))
            .Produces<AuctionDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateAuction)
            .WithName("UpdateAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Edit))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", DeleteAuction)
            .WithName("DeleteAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Delete))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/activate", ActivateAuction)
            .WithName("ActivateAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Edit))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", DeactivateAuction)
            .WithName("DeactivateAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Edit))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/cancel", CancelAuction)
            .WithName("CancelAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Edit))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/extend", ExtendAuction)
            .WithName("ExtendAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Edit))
            .Produces<ExtendAuctionResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
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
            Title: dto.Title,
            Description: dto.Description,
            Condition: dto.Condition,
            YearManufactured: dto.YearManufactured,
            Attributes: dto.Attributes,
            ReservePrice: dto.ReservePrice,
            BuyNowPrice: dto.BuyNowPrice,
            AuctionEnd: dto.AuctionEnd,
            SellerId: sellerId,
            SellerUsername: sellerUsername,
            Currency: dto.Currency,
            Files: commandFiles,
            CategoryId: dto.CategoryId,
            BrandId: dto.BrandId,
            IsFeatured: dto.IsFeatured);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetAuctionById", new { id = result.Value!.Id }, new { id = result.Value.Id })
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> UpdateAuction(
        Guid id,
        UpdateAuctionDto dto,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new UpdateAuctionCommand(
            Id: id,
            Title: dto.Title,
            Description: dto.Description,
            Condition: dto.Condition,
            YearManufactured: dto.YearManufactured,
            Attributes: dto.Attributes,
            ReservePrice: dto.ReservePrice,
            BuyNowPrice: dto.BuyNowPrice,
            CategoryId: dto.CategoryId,
            BrandId: dto.BrandId,
            IsFeatured: dto.IsFeatured,
            AuctionEnd: dto.AuctionEnd,
            UserId: userId);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> DeleteAuction(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new DeleteAuctionCommand(id, userId);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ActivateAuction(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new ActivateAuctionCommand(id, userId);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> DeactivateAuction(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new DeactivateAuctionCommand(id, userId, null);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> CancelAuction(
        Guid id,
        CancelAuctionDto dto,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new CancelAuctionCommand(id, userId, dto.Reason);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ExtendAuction(
        Guid id,
        ExtendAuctionDto dto,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new ExtendAuctionCommand(id, userId, dto.ExtensionMinutes);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Results.Ok(new ExtendAuctionResponseDto
            {
                AuctionId = id,
                NewAuctionEnd = result.Value
            });
        }

        return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}

