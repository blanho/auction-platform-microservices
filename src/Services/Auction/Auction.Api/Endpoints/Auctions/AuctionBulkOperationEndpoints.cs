#nullable enable
using Auctions.Application.Features.Auctions.QueueAuctionExport;
using Auctions.Application.Features.Auctions.QueueAuctionImport;
using Auctions.Application.Enums;
using Auctions.Domain.Enums;
using AuctionService.Contracts.Commands;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Auctions;

public class AuctionBulkOperationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auctions");

        group.MapPost("/export", QueueExport)
            .WithName("QueueAuctionExport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Export))
            .Produces<BackgroundJobResult>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/import", QueueImport)
            .WithName("QueueAuctionImport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Import))
            .Produces<BackgroundJobResult>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> QueueExport(
        QueueAuctionExportRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new QueueAuctionExportCommand(
            RequestedBy: userId,
            Format: request.Format,
            StatusFilter: request.StatusFilter,
            SellerFilter: request.SellerFilter,
            StartDate: request.StartDate,
            EndDate: request.EndDate);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Accepted(value: result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> QueueImport(
        QueueAuctionImportRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var username = UserHelper.GetUsername(httpContext.User);

        var rows = request.Rows.Select(r => new QueueImportAuctionRow(
            Title: r.Title,
            Description: r.Description,
            Condition: r.Condition,
            YearManufactured: r.YearManufactured,
            ReservePrice: r.ReservePrice,
            BuyNowPrice: r.BuyNowPrice,
            AuctionEnd: r.AuctionEnd,
            CategoryId: r.CategoryId,
            BrandId: r.BrandId,
            Attributes: r.Attributes)).ToList();

        var command = new QueueAuctionImportCommand(
            SellerId: userId,
            SellerUsername: username,
            Currency: request.Currency,
            Rows: rows);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Accepted(value: result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}

public record QueueAuctionExportRequest(
    ExportFormat Format,
    Status? StatusFilter = null,
    string? SellerFilter = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null);

public record QueueAuctionImportRequest(
    string Currency,
    IReadOnlyList<ImportAuctionRowDto> Rows);

public record ImportAuctionRowDto(
    string Title,
    string Description,
    string? Condition,
    int? YearManufactured,
    decimal ReservePrice,
    decimal? BuyNowPrice,
    DateTimeOffset AuctionEnd,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    Dictionary<string, string>? Attributes = null);
