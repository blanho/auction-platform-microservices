using Search.Api.Interfaces;

namespace Search.Api.Services;

public class SearchIndexStartupService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SearchIndexStartupService> _logger;

    public SearchIndexStartupService(
        IServiceScopeFactory scopeFactory,
        ILogger<SearchIndexStartupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var indexManagement = scope.ServiceProvider.GetRequiredService<IIndexManagementService>();

        _logger.LogInformation("Ensuring Elasticsearch index exists on startup");

        var result = await indexManagement.EnsureIndexExistsAsync(cancellationToken);

        if (result.IsFailure)
            _logger.LogError("Failed to ensure Elasticsearch index on startup: {Error}", result.Error);
        else
            _logger.LogInformation("Elasticsearch index ready");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
