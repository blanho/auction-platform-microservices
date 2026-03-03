using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Storage.Api.Helpers;

public static class FileValidationHelper
{
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        [".jpg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        [".jpeg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        [".png"] = [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }],
        [".gif"] = [new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }],
        [".webp"] = [new byte[] { 0x52, 0x49, 0x46, 0x46 }],
        [".pdf"] = [new byte[] { 0x25, 0x50, 0x44, 0x46 }],
        [".xlsx"] = [new byte[] { 0x50, 0x4B, 0x03, 0x04 }],
    };

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

        var signatureError = ValidateFileSignature(file, extension);
        if (signatureError is not null)
        {
            return signatureError;
        }

        return null;
    }

    private static ProblemDetails? ValidateFileSignature(IFormFile file, string extension)
    {
        if (!FileSignatures.TryGetValue(extension, out var signatures))
        {
            return null;
        }

        using var reader = new BinaryReader(file.OpenReadStream());
        var maxSignatureLength = signatures.Max(s => s.Length);
        var headerBytes = reader.ReadBytes(maxSignatureLength);

        var matchesAnySignature = signatures.Any(signature =>
            headerBytes.Length >= signature.Length &&
            headerBytes.AsSpan(0, signature.Length).SequenceEqual(signature));

        if (!matchesAnySignature)
        {
            return ProblemDetailsHelper.Create(
                "Invalid File Content",
                $"File '{file.FileName}' content does not match the expected format for '{extension}'",
                StatusCodes.Status400BadRequest);
        }

        return null;
    }
}
