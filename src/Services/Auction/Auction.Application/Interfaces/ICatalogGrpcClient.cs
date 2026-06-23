namespace Auctions.Application.Interfaces;

public interface ICatalogGrpcClient
{
    Task<(string Name, string Slug)?> GetBrandAsync(Guid brandId, CancellationToken cancellationToken = default);
    Task<(string Name, string Slug)?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
