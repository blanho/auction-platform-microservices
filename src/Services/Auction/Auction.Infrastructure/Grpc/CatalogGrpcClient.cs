using Auctions.Application.Interfaces;
using Catalog.Contracts.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Grpc;

public class CatalogGrpcClient : ICatalogGrpcClient
{
    private readonly CatalogGrpc.CatalogGrpcClient _client;
    private readonly ILogger<CatalogGrpcClient> _logger;

    public CatalogGrpcClient(CatalogGrpc.CatalogGrpcClient client, ILogger<CatalogGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<(string Name, string Slug)?> GetBrandAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetBrandByIdAsync(new GetBrandRequest { Id = brandId.ToString() }, cancellationToken: cancellationToken);
            return (response.Name, response.Slug);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Brand {BrandId} not found via gRPC", brandId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Catalog gRPC service for Brand {BrandId}", brandId);
            throw;
        }
    }

    public async Task<(string Name, string Slug)?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetCategoryByIdAsync(new GetCategoryRequest { Id = categoryId.ToString() }, cancellationToken: cancellationToken);
            return (response.Name, response.Slug);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Category {CategoryId} not found via gRPC", categoryId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Catalog gRPC service for Category {CategoryId}", categoryId);
            throw;
        }
    }
}
