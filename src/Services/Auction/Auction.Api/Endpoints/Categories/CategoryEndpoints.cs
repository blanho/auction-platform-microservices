#nullable enable
using Auctions.Api.Extensions;
using Auctions.Application.Features.Categories.BulkUpdateCategories;
using Auctions.Application.Features.Categories.CreateCategory;
using Auctions.Application.Features.Categories.DeleteCategory;
using Auctions.Application.Features.Categories.UpdateCategory;
using Auctions.Application.Features.Categories.GetCategories;
using Auctions.Application.Features.Categories.GetCategoryById;
using Auctions.Application.DTOs;
using BuildingBlocks.Web.Authorization;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Categories;

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
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithMetadata(new RequireAdminAttribute())
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/bulk-update", BulkUpdateCategories)
            .WithName("BulkUpdateCategories")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<int>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetCategories(
        bool activeOnly,
        bool includeCount,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetCategoriesQuery(activeOnly, includeCount);
        var result = await mediator.Send(query, ct);

        return result.ToOkResult();
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateCategoryCommand(
            dto.Name,
            dto.Slug,
            dto.Icon,
            dto.Description,
            dto.ImageUrl,
            dto.DisplayOrder,
            dto.IsActive,
            dto.ParentCategoryId);

        var result = await mediator.Send(command, ct);

        return result.ToApiResult(category => 
            Results.CreatedAtRoute("GetCategories", new { id = category.Id }, category));
    }

    private static async Task<IResult> UpdateCategory(
        Guid id,
        UpdateCategoryDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(
            id,
            dto.Name,
            dto.Slug,
            dto.Icon,
            dto.Description,
            dto.ImageUrl,
            dto.DisplayOrder,
            dto.IsActive,
            dto.ParentCategoryId);

        var result = await mediator.Send(command, ct);

        return result.ToOkResult();
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await mediator.Send(command, ct);

        return result.ToNoContentResult();
    }

    private static async Task<IResult> BulkUpdateCategories(
        BulkUpdateCategoriesDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new BulkUpdateCategoriesCommand(dto.CategoryIds, dto.IsActive);
        var result = await mediator.Send(command, ct);

        return result.ToOkResult();
    }


    private static async Task<IResult> GetCategoryById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await mediator.Send(query, ct);

        return result.ToOkResult();
    }
}

