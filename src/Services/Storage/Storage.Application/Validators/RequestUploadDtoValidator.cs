using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Storage.Application.DTOs;

namespace Storage.Application.Validators;

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
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("File name"))
            .MaximumLength(255).WithMessage(ValidationConstants.Messages.MaxLength("File name", 255))
            .Must(BeValidFileName).WithMessage(ValidationConstants.Messages.InvalidFormat("File name"));

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Content type"))
            .Must(BeAllowedContentType).WithMessage("File type is not allowed");

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSize).WithMessage($"File size must not exceed {MaxFileSize / 1024 / 1024}MB");

        RuleFor(x => x.OwnerService)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Owner service"))
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Owner service", ValidationConstants.StringLength.Standard));
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
