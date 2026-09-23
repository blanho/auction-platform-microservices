using Catalog.Application.Errors;

namespace Catalog.Application.Features.Brands.DeleteBrand;

public class DeleteBrandCommandHandler : ICommandHandler<DeleteBrandCommand>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBrandCommandHandler> _logger;

    public DeleteBrandCommandHandler(
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteBrandCommandHandler> logger)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand is null)
            return Result.Failure(CatalogErrors.Brand.NotFound);

        await _brandRepository.DeleteAsync(request.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand {BrandId} deleted", request.Id);

        return Result.Success();
    }
}
