using Catalog.Contracts.Grpc;
using Catalog.Application.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Grpc;

public class CatalogGrpcService : CatalogGrpc.CatalogGrpcBase
{
    private readonly IBrandRepository _brandRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CatalogGrpcService> _logger;

    public CatalogGrpcService(
        IBrandRepository brandRepository,
        ICategoryRepository categoryRepository,
        ILogger<CatalogGrpcService> logger)
    {
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public override async Task<BrandResponse> GetBrandById(GetBrandRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetBrandById called for Id: {Id}", request.Id);

        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Brand Id format"));
        }

        var brand = await _brandRepository.GetByIdAsync(id, context.CancellationToken);

        if (brand == null || !brand.IsActive)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Brand with Id {request.Id} was not found or is inactive."));
        }

        return new BrandResponse
        {
            Id = brand.Id.ToString(),
            Name = brand.Name,
            Slug = brand.Slug
        };
    }

    public override async Task<CategoryResponse> GetCategoryById(GetCategoryRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetCategoryById called for Id: {Id}", request.Id);

        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Category Id format"));
        }

        var category = await _categoryRepository.GetByIdAsync(id, context.CancellationToken);

        if (category == null || !category.IsActive)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Category with Id {request.Id} was not found or is inactive."));
        }

        return new CategoryResponse
        {
            Id = category.Id.ToString(),
            Name = category.Name,
            Slug = category.Slug
        };
    }
}
