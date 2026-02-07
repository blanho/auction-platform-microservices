using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Storage.Api.Helpers;

public static class FileValidationHelper
{
    public static ProblemDetails? ValidateFile(IFormFile file, FileValidationSettings validation)
    {
        if (file.Length > validation.MaxFileSizeBytes)
        {
            return ProblemDetailsHelper.Create(
                "File Too Large",
                $"File '{file.FileName}' exceeds maximum size of {validation.MaxFileSizeBytes / 1024 / 1024}MB",
                StatusCodes.Status400BadRequest);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (validation.AllowedExtensions.Length > 0 && !validation.AllowedExtensions.Contains(extension))
        {
            return ProblemDetailsHelper.Create(
                "Invalid File Type",
                $"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", validation.AllowedExtensions)}",
                StatusCodes.Status400BadRequest);
        }

        if (validation.AllowedContentTypes.Length > 0 && !validation.AllowedContentTypes.Contains(file.ContentType))
        {
            return ProblemDetailsHelper.Create(
                "Invalid Content Type",
                $"Content type '{file.ContentType}' is not allowed",
                StatusCodes.Status400BadRequest);
        }

        return null;
    }
}
