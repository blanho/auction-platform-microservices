using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BuildingBlocks.Application.Paging;
using Xunit;

namespace Architecture.Tests;

public sealed class SortMapBuilderTests
{
    [Fact]
    public void PublicContractAlias_CanTargetADifferentEntityPropertyWithoutExposingOtherFields()
    {
        var map = new SortMapBuilder<Entity, Contract>()
            .Add(field => field.DisplayName, entity => entity.DatabaseName)
            .Build();
        var rows = new[] { new Entity(2, "Alpha"), new Entity(1, "Zulu") }.AsQueryable();
        var sorted = rows.ApplySorting(new QueryParameters { SortBy = " DISPLAY-NAME ", SortDescending = false },
            map, row => row.Id);
        Assert.Equal(new[] { 2, 1 }, sorted.Select(row => row.Id));
        var unsupported = rows.ApplySorting(new QueryParameters { SortBy = nameof(Entity.Secret), SortDescending = false },
            map, row => row.Id);
        Assert.Equal(new[] { 1, 2 }, unsupported.Select(row => row.Id));
    }

    [Fact]
    public void BuiltMap_IsAnImmutableSnapshotOfTheRegistration()
    {
        var builder = new SortMapBuilder<Entity, Contract>().Add(field => field.Id, entity => entity.Id);
        var map = builder.Build();
        builder.Add(field => field.DisplayName, entity => entity.DatabaseName);
        Assert.Single(map);
        Assert.True(map.ContainsKey("ID"));
        Assert.False(map.ContainsKey("display-name"));
        var dictionary = Assert.IsAssignableFrom<IDictionary<string, Expression<Func<Entity, object>>>>(map);
        Assert.Throws<NotSupportedException>(() => dictionary.Add("changed", entity => entity.Id));
    }

    [Fact]
    public void DuplicatePublicNames_AreRejectedCaseInsensitivelyDuringRegistration()
    {
        var builder = new SortMapBuilder<Entity, Contract>().Add(field => field.DisplayName, entity => entity.DatabaseName);
        Assert.Throws<ArgumentException>(() => builder.Add(field => field.DuplicateName, entity => entity.DatabaseName));
    }

    [Fact]
    public void InvalidContractExpressionsAndNullSelectors_AreRejectedDuringRegistration()
    {
        var builder = new SortMapBuilder<Entity, Contract>();
        Assert.Throws<ArgumentException>(() => builder.Add(field => field.DisplayName.ToLowerInvariant(), entity => entity.DatabaseName));
        var captured = new Contract();
        Assert.Throws<ArgumentException>(() => builder.Add(_ => captured.DisplayName, entity => entity.DatabaseName));
        Assert.Throws<ArgumentNullException>(() => builder.Add<string, string>(null!, entity => entity.DatabaseName));
        Assert.Throws<ArgumentNullException>(() => builder.Add<string, string>(field => field.DisplayName, null!));
    }

    [Fact]
    public void NullablePolicyAndValueTypes_KeepTheirOrderingAndUniqueTieBreaker()
    {
        var map = new SortMapBuilder<Entity, Contract>()
            .Add(field => field.LastSeen, entity => entity.LastSeen ?? DateTimeOffset.MinValue)
            .Add(field => field.Id, entity => entity.Id)
            .Build();
        var rows = new[]
        {
            new Entity(3, "recent", DateTimeOffset.UtcNow), new Entity(2, "never"), new Entity(1, "never")
        }.AsQueryable();
        var sorted = rows.ApplySorting(new QueryParameters { SortBy = nameof(Contract.LastSeen), SortDescending = false },
            map, row => row.Id, tieBreaker: row => row.Id);
        Assert.Equal(new[] { 1, 2, 3 }, sorted.Select(row => row.Id));
    }

    [Fact]
    public void StringContractField_PreservesNativeEnumKeyOrdering()
    {
        var map = new SortMapBuilder<StatusEntity, StatusContract>()
            .Add(field => field.Status, entity => entity.Status)
            .Build();
        var rows = new[] { new StatusEntity(State.Alpha), new StatusEntity(State.Zulu) }.AsQueryable();
        var sorted = rows.ApplySorting(new QueryParameters { SortBy = nameof(StatusContract.Status), SortDescending = false }, map);

        Assert.Equal(new[] { State.Zulu, State.Alpha }, sorted.Select(row => row.Status));
    }

    private enum State { Zulu, Alpha }
    private sealed record StatusEntity(State Status);
    private sealed record StatusContract(string Status);

    private sealed record Entity(int Id, string DatabaseName, DateTimeOffset? LastSeen = null)
    {
        public string Secret => DatabaseName;
    }

    private sealed class Contract
    {
        public int Id { get; init; }
        [JsonPropertyName("display-name")]
        public string DisplayName { get; init; } = string.Empty;
        [JsonPropertyName("DISPLAY-NAME")]
        public string DuplicateName { get; init; } = string.Empty;
        public DateTimeOffset? LastSeen { get; init; }
    }
}
