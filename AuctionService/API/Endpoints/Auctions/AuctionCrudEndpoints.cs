#nullable enable
using AuctionService.Application.Commands.ActivateAuction;
using AuctionService.Application.Commands.CreateAuction;
using AuctionService.Application.Commands.DeactivateAuction;
using AuctionService.Application.Commands.DeleteAuction;
using AuctionService.Application.Commands.UpdateAuction;
using AuctionService.Application.DTOs;
using AuctionService.Application.DTOs.Requests;
using Carter;
using Common.Core.Authorization;
using Common.Core.Helpers;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Endpoints.Auctions;

public class AuctionCrudEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auctions")
            .WithOpenApi();

        group.MapPost("/", CreateAuction)
            .WithName("CreateAuction")
            .RequireAuthorization($"Permission:{Permissions.Auctions.Create}")
            .Produces<AuctionDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateAuction)
            .WithName("UpdateAuction")
            .RequireAuthorization($"Permission:{Permissions.Auctions.Edit}")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", DeleteAuction)
            .WithName("DeleteAuction")
            .RequireAuthorization($"Permission:{Permissions.Auctions.Delete}")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/activate", ActivateAuction)
            .WithName("ActivateAuction")
            .RequireAuthorization($"Permission:{Permissions.Auctions.Edit}")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", DeactivateAuction)
            .WithName("DeactivateAuction")
            .RequireAuthorization($"Permission:{Permissions.Auctions.Edit}")
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
