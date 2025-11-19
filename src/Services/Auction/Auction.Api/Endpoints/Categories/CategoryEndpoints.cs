#nullable enable
using Auctions.Application.Commands.BulkUpdateCategories;
using Auctions.Application.Commands.CreateCategory;
using Auctions.Application.Commands.DeleteCategory;
using Auctions.Application.Commands.ImportCategories;
using Auctions.Application.Commands.UpdateCategory;
using Auctions.Application.DTOs;
using Auctions.Application.Queries.GetCategories;
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

        group.MapPost("/import", ImportCategories)
            .WithName("ImportCategories")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<ImportCategoriesResultDto>(StatusCodes.Status200OK)
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

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
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

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetCategories", new { id = result.Value!.Id }, result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
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

        if (!result.IsSuccess)
        {
            return result.Error?.Code == "Category.NotFound"
                ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
                : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code == "Category.NotFound"
                ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
                : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.NoContent();
    }

    private static async Task<IResult> BulkUpdateCategories(
        BulkUpdateCategoriesDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new BulkUpdateCategoriesCommand(dto.CategoryIds, dto.IsActive);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ImportCategories(
        ImportCategoriesDto dto,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new ImportCategoriesCommand(dto.Categories);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        return Results.Ok(new ImportCategoriesResultDto
        {
            SuccessCount = result.Value!.SuccessCount,
            FailureCount = result.Value.FailureCount,
            Errors = result.Value.Errors
        });
    }
}

