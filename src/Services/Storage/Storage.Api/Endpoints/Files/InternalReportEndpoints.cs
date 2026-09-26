using System.Security.Cryptography;
using System.Text;
using Carter;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Storage.Application.Features.Files.UploadFile;
using Storage.Application.Interfaces;
using StorageService.Contracts.Reports;

namespace Storage.Api.Endpoints.Files;

public sealed class InternalReportEndpoints : ICarterModule
{
    private const long MaxReportSizeBytes = 50 * 1024 * 1024;

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ReportStorageContract.UploadPath, UploadReport)
            .AllowAnonymous()
            .WithMetadata(new RequestSizeLimitAttribute(MaxReportSizeBytes))
            .ExcludeFromDescription();

        app.MapGet("/api/v1/files/{fileId:guid}/download", DownloadReport)
            .RequireAuthorization();
    }

    private static async Task<IResult> DownloadReport(
        Guid fileId,
        HttpContext context,
        IStoredFileRepository repository,
        IFileStorageService fileStorage,
        CancellationToken cancellationToken)
    {
        var ownerId = UserHelper.GetUserId(context.User);
        var storedFile = await repository.GetByIdAsync(fileId, cancellationToken);
        if (ownerId is null || storedFile is null || storedFile.IsDeleted ||
            storedFile.OwnerId != ownerId ||
            storedFile.SubFolder != ReportStorageContract.PrivateFolder)
            return Results.NotFound();

        var download = await fileStorage.DownloadAsync(storedFile.StoredFileName, cancellationToken);
        return download is null
            ? Results.NotFound()
            : Results.File(download.Content, download.ContentType, storedFile.FileName);
    }

    private static async Task<IResult> UploadReport(
        HttpContext context,
        IConfiguration configuration,
        ISender sender,
        IStoredFileRepository repository,
        CancellationToken cancellationToken)
    {
        var configuredKey = configuration["ReportStorage:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        var suppliedKey = context.Request.Headers[ReportStorageContract.ApiKeyHeader].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(configuredKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
        if (expectedBytes.Length != suppliedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
            return Results.Unauthorized();

        var fileName = context.Request.Headers[ReportStorageContract.FileNameHeader].ToString();
        var ownerIdText = context.Request.Headers[ReportStorageContract.OwnerIdHeader].ToString();
        var contentType = context.Request.ContentType;
        var fileSize = context.Request.ContentLength;

        if (!Guid.TryParse(ownerIdText, out var ownerId) || ownerId == Guid.Empty ||
            string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(['/', '\\']) >= 0 ||
            !IsSupportedReport(fileName, contentType) ||
            fileSize is null or <= 0 or > MaxReportSizeBytes)
            return Results.BadRequest();

        var result = await sender.Send(new UploadFileCommand(
            context.Request.Body, fileName, contentType!, fileSize.Value,
            ReportStorageContract.PrivateFolder, ownerId), cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest();

        var storedFile = await repository.GetByIdAsync(result.Value!.FileId, cancellationToken);
        if (storedFile is null)
            return Results.StatusCode(StatusCodes.Status500InternalServerError);

        return Results.Ok(new StoredReportResponse(
            storedFile.Id, $"/files/{storedFile.Id}/download"));
    }

    private static bool IsSupportedReport(string fileName, string? contentType)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".csv" => contentType == "text/csv",
            ".pdf" => contentType == "application/pdf",
            ".xlsx" => contentType ==
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => false
        };
    }
}
