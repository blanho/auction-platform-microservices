using Auctions.Application.DTOs.Audit;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Constants;
using Xunit;

namespace Auction.Application.Tests;

public sealed class AuctionAuditMetadataTests
{
    [Fact]
    public void ForActivation_UsesStableAuditSchema()
    {
        var metadata = AuctionAuditMetadata.ForActivation(Status.Scheduled);

        Assert.Equal("Activated", metadata[AuditMetadataKeys.Action]);
        Assert.Equal(nameof(Status.Scheduled), metadata[AuditMetadataKeys.PreviousStatus]);
    }

    [Fact]
    public void ForExtension_IncludesTimelineDetails()
    {
        var previousEnd = DateTimeOffset.Parse("2026-08-22T10:00:00Z");
        var newEnd = previousEnd.AddMinutes(15);

        var metadata = AuctionAuditMetadata.ForExtension(15, previousEnd, newEnd);

        Assert.Equal("Extended", metadata[AuditMetadataKeys.Action]);
        Assert.Equal(15, metadata[AuditMetadataKeys.ExtensionMinutes]);
        Assert.Equal(previousEnd, metadata[AuditMetadataKeys.PreviousEnd]);
        Assert.Equal(newEnd, metadata[AuditMetadataKeys.NewEnd]);
    }
}
