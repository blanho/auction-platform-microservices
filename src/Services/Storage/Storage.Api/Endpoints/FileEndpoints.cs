using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Storage.Api.DTOs;
using Storage.Application.DTOs;
using Storage.Application.Interfaces;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using System.Security.Claims;

namespace Storage.Api.Endpoints;

public class FileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/files")
            .WithTags("Files")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.View));

        group.MapPost("/request-upload", RequestUpload)
            .WithName("RequestUpload")
            .WithSummary("Request a presigned URL for file upload")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload));

        group.MapPost("/confirm-upload", ConfirmUpload)
            .WithName("ConfirmUpload")
            .WithSummary("Confirm a file upload")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload));

        group.MapGet("/{fileId:guid}/download-url", GetDownloadUrl)
            .WithName("GetDownloadUrl")
            .WithSummary("Get a presigned download URL for a file");

        group.MapPost("/direct-upload", DirectUpload)
            .WithName("DirectUpload")
            .WithSummary("Upload a file directly")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload))
            .DisableAntiforgery();

        group.MapPost("/direct-upload/batch", DirectUploadBatch)
            .WithName("DirectUploadBatch")
            .WithSummary("Upload multiple files directly")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload))
            .DisableAntiforgery();

        group.MapPost("/confirm", Confirm)
            .WithName("ConfirmFile")
            .WithSummary("Confirm a file")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload));

        group.MapPost("/confirm/batch", ConfirmBatch)
            .WithName("ConfirmFileBatch")
            .WithSummary("Confirm multiple files")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Upload));

        group.MapGet("/{fileId:guid}", GetMetadata)
            .WithName("GetFileMetadata")
            .WithSummary("Get file metadata");

        group.MapGet("/owner/{ownerService}/{ownerId}", GetByOwner)
            .WithName("GetFilesByOwner")
            .WithSummary("Get files by owner");

        group.MapDelete("/{fileId:guid}", Delete)
            .WithName("DeleteFile")
            .WithSummary("Delete a file")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Storage.Delete));
    }

    private static async Task<Results<Ok<UploadUrlResponse>, BadRequest<string>>> RequestUpload(
        RequestUploadDto request,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        try
        {
            var uploadedBy = user.Identity?.Name;
            var response = await fileStorageService.RequestUploadAsync(request, uploadedBy, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<Ok<FileMetadataDto>, BadRequest<string>>> ConfirmUpload(
        ConfirmUploadRequest request,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var result = await fileStorageService.ConfirmUploadAsync(request, cancellationToken);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.Error ?? "Unknown error");
        }

        return TypedResults.Ok(result.Metadata!);
    }

    private static async Task<Results<Ok<DownloadUrlResponse>, NotFound<ProblemDetails>>> GetDownloadUrl(
        Guid fileId,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var response = await fileStorageService.GetDownloadUrlAsync(fileId, cancellationToken);

        if (response == null)
        {
            return TypedResults.NotFound(ProblemDetailsHelper.Create(
                "File.NotFound",
                $"File with ID {fileId} was not found",
                StatusCodes.Status404NotFound));
        }

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<FileMetadataDto>, BadRequest<string>>> DirectUpload(
        IFormFile file,
        [FromQuery] string ownerService,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return TypedResults.BadRequest("No file provided");
        }

        if (string.IsNullOrEmpty(ownerService))
        {
            return TypedResults.BadRequest("Owner service is required");
        }

        var uploadedBy = user.Identity?.Name;

        await using var stream = file.OpenReadStream();
        var result = await fileStorageService.DirectUploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            ownerService,
            uploadedBy,
            cancellationToken);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.Error ?? "Unknown error");
        }

        return TypedResults.Ok(result.Metadata!);
    }

    private static async Task<Results<Ok<BatchUploadResult>, BadRequest<string>>> DirectUploadBatch(
        IFormFileCollection files,
        [FromQuery] string ownerService,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
        {
            return TypedResults.BadRequest("No files provided");
        }

        if (string.IsNullOrEmpty(ownerService))
        {
            return TypedResults.BadRequest("Owner service is required");
        }

        var uploadedBy = user.Identity?.Name;
        var results = new List<FileMetadataDto>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var result = await fileStorageService.DirectUploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                ownerService,
                uploadedBy,
                cancellationToken);

            if (result.Success && result.Metadata != null)
            {
                results.Add(result.Metadata);
            }
            else
            {
                errors.Add($"{file.FileName}: {result.Error}");
            }
        }

        return TypedResults.Ok(new BatchUploadResult(results, errors));
    }

    private static async Task<Results<Ok<FileMetadataDto>, BadRequest<string>>> Confirm(
        FileConfirmRequest request,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var result = await fileStorageService.ConfirmFileAsync(request, cancellationToken);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.Error ?? "Unknown error");
        }

        return TypedResults.Ok(result.Metadata!);
    }

    private static async Task<Ok<BatchConfirmResult>> ConfirmBatch(
        BatchConfirmRequest request,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var results = new List<FileMetadataDto>();
        var errors = new List<string>();

        foreach (var fileRequest in request.Files)
        {
            var result = await fileStorageService.ConfirmUploadAsync(fileRequest, cancellationToken);

            if (result.Success && result.Metadata != null)
            {
                results.Add(result.Metadata);
            }
            else
            {
                errors.Add($"{fileRequest.FileId}: {result.Error}");
            }
        }

        return TypedResults.Ok(new BatchConfirmResult(results, errors));
    }

    private static async Task<Results<Ok<FileMetadataDto>, NotFound<ProblemDetails>>> GetMetadata(
        Guid fileId,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var metadata = await fileStorageService.GetMetadataAsync(fileId, cancellationToken);

        if (metadata == null)
        {
            return TypedResults.NotFound(ProblemDetailsHelper.Create(
                "File.NotFound",
                $"File with ID {fileId} was not found",
                StatusCodes.Status404NotFound));
        }

        return TypedResults.Ok(metadata);
    }

    private static async Task<Ok<IEnumerable<FileMetadataDto>>> GetByOwner(
        string ownerService,
        string ownerId,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var files = await fileStorageService.GetByOwnerAsync(ownerService, ownerId, cancellationToken);
        return TypedResults.Ok<IEnumerable<FileMetadataDto>>(files);
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>>> Delete(
        Guid fileId,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var result = await fileStorageService.DeleteAsync(fileId, cancellationToken);

        if (!result)
        {
            return TypedResults.NotFound(ProblemDetailsHelper.Create(
                "File.NotFound",
                $"File with ID {fileId} was not found or could not be deleted",
                StatusCodes.Status404NotFound));
        }

        return TypedResults.NoContent();
    }
}
