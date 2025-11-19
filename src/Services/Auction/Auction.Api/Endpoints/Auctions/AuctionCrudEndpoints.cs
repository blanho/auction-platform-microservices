#nullable enable
using Auctions.Api.Extensions;
using Auctions.Application.Commands.ActivateAuction;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Application.Commands.DeactivateAuction;
using Auctions.Application.Commands.DeleteAuction;
using Auctions.Application.Commands.UpdateAuction;
using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
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
        HttpContext httpContext,
        IMediator mediator,
        IAuctionRepository auctionRepository,
        CancellationToken ct)
    {
        var (auction, error) = await auctionRepository.GetAuthorizedAuctionAsync(
            httpContext, id, Permissions.Auctions.Edit, ct);
        
        if (error != null) return error;
        
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
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> DeleteAuction(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        IAuctionRepository auctionRepository,
        CancellationToken ct)
    {
        var (auction, error) = await auctionRepository.GetAuthorizedAuctionAsync(
            httpContext, id, Permissions.Auctions.Delete, ct);
        
        if (error != null) return error;
        
        var command = new DeleteAuctionCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ActivateAuction(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        IAuctionRepository auctionRepository,
        CancellationToken ct)
    {
        var (auction, error) = await auctionRepository.GetAuthorizedAuctionAsync(
            httpContext, id, Permissions.Auctions.Edit, ct);
        
        if (error != null) return error;

        var command = new ActivateAuctionCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> DeactivateAuction(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        IAuctionRepository auctionRepository,
        CancellationToken ct)
    {
        var (auction, error) = await auctionRepository.GetAuthorizedAuctionAsync(
            httpContext, id, Permissions.Auctions.Edit, ct);
        
        if (error != null) return error;

        var command = new DeactivateAuctionCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }
}

