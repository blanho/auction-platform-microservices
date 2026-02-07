using BuildingBlocks.Application.Abstractions.Storage;
using FluentValidation;

namespace Storage.Application.Features.Files.UploadFile;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator(IOptions<FileStorageSettings> settings)
    {
        var validation = settings.Value.Validation;

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty();

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(validation.MaxFileSizeBytes);

        RuleFor(x => x.Content)
            .NotNull();
    }
}
