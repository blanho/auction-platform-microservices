using System.ComponentModel.DataAnnotations;

namespace Search.Api.Configuration;

public class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    [Required(ErrorMessage = "Elasticsearch URL is required")]
    [Url(ErrorMessage = "Elasticsearch URL must be a valid URL")]
    public string Url { get; set; } = "http://localhost:9200";

    [Required]
    [MinLength(1, ErrorMessage = "Index prefix cannot be empty")]
    public string IndexPrefix { get; set; } = "auctions";

    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableDebugMode { get; set; } = false;

    [Range(1, 300, ErrorMessage = "Request timeout must be between 1 and 300 seconds")]
    public int RequestTimeout { get; set; } = 30;

    [Range(0, 10, ErrorMessage = "Max retries must be between 0 and 10")]
    public int MaxRetries { get; set; } = 3;

    [Range(1, 10, ErrorMessage = "Number of shards must be between 1 and 10")]
    public int NumberOfShards { get; set; } = 1;

    [Range(0, 5, ErrorMessage = "Number of replicas must be between 0 and 5")]
    public int NumberOfReplicas { get; set; } = 0;

    public string GetIndexName() => IndexPrefix.ToLowerInvariant();
}

public class SearchOptions
{
    public const string SectionName = "Search";

    [Range(1, 100, ErrorMessage = "Default page size must be between 1 and 100")]
    public int DefaultPageSize { get; set; } = 20;

    [Range(1, 1000, ErrorMessage = "Max page size must be between 1 and 1000")]
    public int MaxPageSize { get; set; } = 100;

    [Range(100, 100000, ErrorMessage = "Max result window must be between 100 and 100000")]
    public int MaxResultWindow { get; set; } = 10000;

    [Range(0, 60, ErrorMessage = "Staleness tolerance must be between 0 and 60 seconds")]
    public int StalenessToleranceSeconds { get; set; } = 5;
}
