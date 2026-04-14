using BuildingBlocks.Application.Abstractions;

namespace BuildingBlocks.Application.Abstractions;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);
    Task<bool> IsEnabledForUserAsync(string featureName, Guid userId, CancellationToken cancellationToken = default);
    Task<T?> GetValueAsync<T>(string featureName, CancellationToken cancellationToken = default);
}
