using System.Reflection;
using Bidding.Application.Features.AutoBids.GetAutoBid;
using Bidding.Application.Interfaces;
using Bidding.Domain.Entities;
using Xunit;

namespace Bidding.Application.Tests;

public class AutoBidOwnershipTests
{
    [Fact]
    public async Task GetAutoBid_WhenCallerIsNotOwner_ReturnsNotFoundBeforeLoadingBidHistory()
    {
        var autoBid = AutoBid.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "owner",
            500m);
        var repository = DispatchProxy.Create<IAutoBidRepository, AutoBidRepositoryProxy>();
        ((AutoBidRepositoryProxy)(object)repository).AutoBid = autoBid;
        var handler = new GetAutoBidQueryHandler(
            repository,
            bidRepository: null!,
            logger: Microsoft.Extensions.Logging.Abstractions.NullLogger<GetAutoBidQueryHandler>.Instance);

        var result = await handler.Handle(
            new GetAutoBidQuery(autoBid.Id, Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AutoBid.NotFound", result.Error?.Code);
    }

    public class AutoBidRepositoryProxy : DispatchProxy
    {
        public AutoBid? AutoBid { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            targetMethod?.Name switch
            {
                nameof(IAutoBidRepository.GetByIdAsync) => Task.FromResult(AutoBid),
                _ => throw new NotSupportedException(
                    $"Repository member '{targetMethod?.Name}' is not used by this test")
            };
    }
}
