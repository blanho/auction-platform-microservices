using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Infrastructure.FeatureFlags;

public sealed class ConfigurationFeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;
    private const string SectionName = "FeatureFlags";

    public ConfigurationFeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var value = _configuration.GetValue<bool>($"{SectionName}:{featureName}");
        return Task.FromResult(value);
    }

    public Task<bool> IsEnabledForUserAsync(string featureName, Guid userId, CancellationToken cancellationToken = default)
    {
        return IsEnabledAsync(featureName, cancellationToken);
    }

    public Task<T?> GetValueAsync<T>(string featureName, CancellationToken cancellationToken = default)
    {
        var value = _configuration.GetValue<T>($"{SectionName}:{featureName}");
        return Task.FromResult(value);
    }
}
