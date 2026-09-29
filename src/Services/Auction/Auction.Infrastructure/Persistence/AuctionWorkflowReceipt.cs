namespace Auctions.Infrastructure.Persistence;

public class AuctionWorkflowReceipt
{
    public string Key { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public string ErrorsJson { get; set; } = "[]";
}
