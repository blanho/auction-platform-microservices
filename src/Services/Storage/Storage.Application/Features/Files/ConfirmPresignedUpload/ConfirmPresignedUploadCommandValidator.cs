using FluentValidation;

namespace Storage.Application.Features.Files.ConfirmPresignedUpload;

public class ConfirmPresignedUploadCommandValidator : AbstractValidator<ConfirmPresignedUploadCommand>
{
    public ConfirmPresignedUploadCommandValidator()
    {
        RuleFor(x => x.StoredFileName).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.FileSize).GreaterThan(0);
    }
}
