#nullable enable
using Auctions.Application.Features.Brands.CreateBrand;
using Auctions.Application.Features.Brands.DeleteBrand;
using Auctions.Application.Features.Brands.UpdateBrand;
using Auctions.Application.Features.Brands.GetBrandById;
using Auctions.Application.Features.Brands.GetBrands;
using Auctions.Application.DTOs;
using BuildingBlocks.Web.Authorization;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Brands;

public class BrandEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/brands")
            .WithTags("Brands")
            .WithOpenApi();

        group.MapGet("/", GetBrands)
            .WithName("GetBrands")
            .AllowAnonymous()
            .Produces<List<BrandDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetBrandById)
            .WithName("GetBrandById")
            .AllowAnonymous()
            .Produces<BrandDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBrand)
            .WithName("CreateBrand")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<BrandDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateBrand)
            .WithName("UpdateBrand")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<BrandDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", DeleteBrand)
            .WithName("DeleteBrand")
            .WithMetadata(new RequireAdminAttribute())
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetBrands(
        bool activeOnly,
        bool featuredOnly,
        int? count,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBrandsQuery(activeOnly, featuredOnly, count);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetBrandById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBrandByIdQuery(id);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code == "Brand.NotFound"
                ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
                : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateBrand(
        CreateBrandDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateBrandCommand(
            dto.Name,
            dto.LogoUrl,
            dto.Description,
            dto.DisplayOrder,
            dto.IsFeatured);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetBrandById", new { id = result.Value!.Id }, result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> UpdateBrand(
        Guid id,
        UpdateBrandDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateBrandCommand(
            id,
            dto.Name,
            dto.LogoUrl,
            dto.Description,
            dto.DisplayOrder,
            dto.IsActive,
            dto.IsFeatured);

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code == "Brand.NotFound"
                ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
                : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteBrand(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeleteBrandCommand(id);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code == "Brand.NotFound"
                ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
                : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.NoContent();
    }
}

