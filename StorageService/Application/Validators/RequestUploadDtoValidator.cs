using FluentValidation;
using StorageService.Application.DTOs;

namespace StorageService.Application.Validators;

public class RequestUploadDtoValidator : AbstractValidator<RequestUploadDto>
{
    private static readonly string[] AllowedContentTypes = 
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp",
        "application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private const long MaxFileSize = 50 * 1024 * 1024;

    public RequestUploadDtoValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name must not exceed 255 characters")
            .Must(BeValidFileName).WithMessage("File name contains invalid characters");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(BeAllowedContentType).WithMessage("File type is not allowed");

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSize).WithMessage($"File size must not exceed {MaxFileSize / 1024 / 1024}MB");

        RuleFor(x => x.OwnerService)
            .NotEmpty().WithMessage("Owner service is required")
            .MaximumLength(100).WithMessage("Owner service must not exceed 100 characters");
    }

    private static bool BeValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        return !fileName.Any(c => invalidChars.Contains(c));
    }

    private static bool BeAllowedContentType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
}
