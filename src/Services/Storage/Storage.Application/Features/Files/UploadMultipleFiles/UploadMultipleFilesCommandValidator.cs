using BuildingBlocks.Application.Abstractions.Storage;
using FluentValidation;

namespace Storage.Application.Features.Files.UploadMultipleFiles;

public class UploadMultipleFilesCommandValidator : AbstractValidator<UploadMultipleFilesCommand>
{
    public UploadMultipleFilesCommandValidator(IOptions<FileStorageSettings> settings)
    {
        var validation = settings.Value.Validation;

        RuleFor(x => x.Files)
            .NotEmpty()
            .Must(files => files.Count <= validation.MaxFilesPerUpload)
            .WithMessage($"Maximum {validation.MaxFilesPerUpload} files allowed per upload");

        RuleForEach(x => x.Files).ChildRules(file =>
        {
            file.RuleFor(f => f.FileName).NotEmpty().MaximumLength(255);
            file.RuleFor(f => f.ContentType).NotEmpty();
            file.RuleFor(f => f.FileSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(validation.MaxFileSizeBytes);
            file.RuleFor(f => f.Content).NotNull();
        });
    }
}
