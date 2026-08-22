#nullable enable
using Carter;
using Catalog.Application.Features.Brands.CreateBrand;
using Catalog.Application.Features.Brands.DeleteBrand;
using Catalog.Application.Features.Brands.GetBrandById;
using Catalog.Application.Features.Brands.GetBrands;
using Catalog.Application.Features.Brands.UpdateBrand;
using Catalog.Application.DTOs;
using BuildingBlocks.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints.Brands;

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
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteBrand)
            .WithName("DeleteBrand")
            .WithMetadata(new RequireAdminAttribute())
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetBrands(
        bool activeOnly = true,
        bool featuredOnly = false,
        int? count = null,
        IMediator mediator = null!,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetBrandsQuery(activeOnly, featuredOnly, count), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> GetBrandById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBrandByIdQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> CreateBrand(CreateBrandDto dto, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateBrandCommand(dto.Name, dto.Description, dto.DisplayOrder, dto.IsFeatured), ct);
        if (!result.IsSuccess)
            return Results.BadRequest(result.Error);
        return Results.CreatedAtRoute("GetBrandById", new { id = result.Value!.Id }, result.Value);
    }

    private static async Task<IResult> UpdateBrand(Guid id, UpdateBrandDto dto, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateBrandCommand(id, dto.Name, dto.Description, dto.DisplayOrder, dto.IsActive, dto.IsFeatured), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> DeleteBrand(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteBrandCommand(id), ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }
}
