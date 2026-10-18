namespace SearchService.Contracts.Commands;

public record ReindexAuctionsCommand
{
    public List<Guid> AuctionIds { get; init; } = [];
    public Guid RequestedBy { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid? ParentJobId { get; init; }
    public Guid? ParentJobItemId { get; init; }
    public int Attempt { get; init; }
}
