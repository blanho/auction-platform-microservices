using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Storage.Api.Helpers;
using Storage.Application.DTOs;
using Storage.Application.Features.Files.DeleteFile;
using Storage.Application.Features.Files.GetFileById;
using Storage.Application.Features.Files.GetFileUrl;
using Storage.Application.Features.Files.UploadFile;
using Storage.Application.Features.Files.UploadMultipleFiles;
using Microsoft.Extensions.Options;

namespace Storage.Api.Endpoints.Files;

public class FileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/files")
            .WithTags("Files")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.View))
            .DisableAntiforgery();

        group.MapPost("/", UploadFile)
            .WithName("UploadFile")
            .WithSummary("Upload a single file")
            .WithOpenApi()
            .Accepts<IFormFile>("multipart/form-data")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload));

        group.MapPost("/batch", UploadMultipleFiles)
            .WithName("UploadMultipleFiles")
            .WithSummary("Upload multiple files")
            .WithOpenApi()
            .Accepts<IFormFileCollection>("multipart/form-data")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload));

        group.MapGet("/{fileId:guid}", GetFile)
            .WithName("GetFile")
            .WithSummary("Get file metadata by ID")
            .WithOpenApi();

        group.MapGet("/{fileId:guid}/url", GetFileUrl)
            .WithName("GetFileUrl")
            .WithSummary("Get file download URL")
            .WithOpenApi();

        group.MapDelete("/{fileId:guid}", DeleteFile)
            .WithName("DeleteFile")
            .WithSummary("Delete a file")
            .WithOpenApi()
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Delete));
    }

    private static async Task<Results<Ok<StoredFileDto>, BadRequest<ProblemDetails>>> UploadFile(
        HttpContext httpContext,
        ISender sender,
        IOptions<FileStorageSettings> settings,
        CancellationToken cancellationToken)
    {
        var form = await httpContext.Request.ReadFormAsync(cancellationToken);
        var file = form.Files.FirstOrDefault();

        if (file is null || file.Length == 0)
        {
            return TypedResults.BadRequest(ProblemDetailsHelper.Create(
                "Validation Error",
                "No file provided",
                StatusCodes.Status400BadRequest));
        }

        var validationError = FileValidationHelper.ValidateFile(file, settings.Value.Validation);
        if (validationError is not null)
        {
            return TypedResults.BadRequest(validationError);
        }

        var subFolder = form["subFolder"].FirstOrDefault();
        Guid? ownerId = Guid.TryParse(form["ownerId"].FirstOrDefault(), out var parsedOwnerId)
            ? parsedOwnerId
            : UserHelper.GetUserId(httpContext.User);

        var command = new UploadFileCommand(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length,
            subFolder,
            ownerId);

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<BatchUploadResultDto>, BadRequest<ProblemDetails>>> UploadMultipleFiles(
        HttpContext httpContext,
        ISender sender,
        IOptions<FileStorageSettings> settings,
        CancellationToken cancellationToken)
    {
        var form = await httpContext.Request.ReadFormAsync(cancellationToken);
        var files = form.Files;

        if (files.Count == 0)
        {
            return TypedResults.BadRequest(ProblemDetailsHelper.Create(
                "Validation Error",
                "No files provided",
                StatusCodes.Status400BadRequest));
        }

        var validation = settings.Value.Validation;

        if (files.Count > validation.MaxFilesPerUpload)
        {
            return TypedResults.BadRequest(ProblemDetailsHelper.Create(
                "Validation Error",
                $"Maximum {validation.MaxFilesPerUpload} files allowed per upload",
                StatusCodes.Status400BadRequest));
        }

        foreach (var file in files)
        {
            var validationError = FileValidationHelper.ValidateFile(file, validation);
            if (validationError is not null)
            {
                return TypedResults.BadRequest(validationError);
            }
        }

        var subFolder = form["subFolder"].FirstOrDefault();
        Guid? ownerId = Guid.TryParse(form["ownerId"].FirstOrDefault(), out var parsedOwnerId)
            ? parsedOwnerId
            : UserHelper.GetUserId(httpContext.User);

        var fileItems = files.Select(f => new UploadFileItem(
            f.OpenReadStream(),
            f.FileName,
            f.ContentType,
            f.Length)).ToList();

        var command = new UploadMultipleFilesCommand(fileItems, subFolder, ownerId);
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<StoredFileDto>, NotFound>> GetFile(
        Guid fileId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetFileByIdQuery(fileId);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<FileUrlDto>, NotFound>> GetFileUrl(
        Guid fileId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetFileUrlQuery(fileId);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteFile(
        Guid fileId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFileCommand(fileId);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
