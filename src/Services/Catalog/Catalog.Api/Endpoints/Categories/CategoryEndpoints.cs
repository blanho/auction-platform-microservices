#nullable enable
using Carter;
using Catalog.Application.Features.Categories.CreateCategory;
using Catalog.Application.Features.Categories.DeleteCategory;
using Catalog.Application.Features.Categories.GetCategories;
using Catalog.Application.Features.Categories.GetCategoryById;
using Catalog.Application.Features.Categories.GetCategoryTree;
using Catalog.Application.Features.Categories.UpdateCategory;
using Catalog.Application.DTOs;
using BuildingBlocks.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints.Categories;

public class CategoryEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories")
            .WithOpenApi();

        group.MapGet("/", GetCategories)
            .WithName("GetCategories")
            .AllowAnonymous()
            .Produces<List<CategoryDto>>(StatusCodes.Status200OK);

        group.MapGet("/tree", GetCategoryTree)
            .WithName("GetCategoryTree")
            .AllowAnonymous()
            .Produces<List<CategoryTreeDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetCategoryById)
            .WithName("GetCategoryById")
            .AllowAnonymous()
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<CategoryDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateCategory)
            .WithName("UpdateCategory")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithMetadata(new RequireAdminAttribute())
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetCategories(bool activeOnly = true, IMediator mediator = null!, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCategoriesQuery(activeOnly), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetCategoryTree(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryTreeQuery(), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetCategoryById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryByIdQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> CreateCategory(CreateCategoryDto dto, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCategoryCommand(dto.Name, dto.Slug, dto.Icon, dto.Description, dto.DisplayOrder, dto.ParentCategoryId), ct);
        if (!result.IsSuccess)
            return Results.BadRequest(result.Error);
        return Results.CreatedAtRoute("GetCategoryById", new { id = result.Value!.Id }, result.Value);
    }

    private static async Task<IResult> UpdateCategory(Guid id, UpdateCategoryDto dto, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateCategoryCommand(id, dto.Name, dto.Slug, dto.Icon, dto.Description, dto.DisplayOrder, dto.IsActive, dto.ParentCategoryId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> DeleteCategory(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCategoryCommand(id), ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }
}
