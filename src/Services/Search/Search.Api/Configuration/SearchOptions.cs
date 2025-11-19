namespace Search.Api.Configuration;

public class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; set; } = "http://localhost:9200";
    public string IndexPrefix { get; set; } = "auctions";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableDebugMode { get; set; } = false;
    public int RequestTimeout { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int NumberOfShards { get; set; } = 1;
    public int NumberOfReplicas { get; set; } = 0;

    public string GetIndexName() => IndexPrefix.ToLowerInvariant();
}

public class SearchOptions
{
    public const string SectionName = "Search";

    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;

    public int MaxResultWindow { get; set; } = 10000;

    public int StalenessToleranceSeconds { get; set; } = 5;
}
