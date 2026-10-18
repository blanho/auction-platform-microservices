using BuildingBlocks.Domain.Entities;
using Xunit;

namespace Architecture.Tests;

public sealed class DomainEqualityTests
{
    [Fact]
    public void EntityEquality_UsesTypeAndIdentityAcrossObjectTypedAndOperatorComparisons()
    {
        var id = Guid.NewGuid();
        BaseEntity first = new Entity(id);
        BaseEntity same = new Entity(id);
        BaseEntity otherType = new OtherEntity(id);

        Assert.True(first.Equals((object)same));
        Assert.True(((IEquatable<BaseEntity>)first).Equals(same));
        Assert.True(first == same);
        Assert.Equal(first.GetHashCode(), same.GetHashCode());
        Assert.False(first == otherType);
        Assert.False(first.Equals(new Entity(Guid.Empty)));
        Assert.False(new Entity(Guid.Empty) == new Entity(Guid.Empty));
        Assert.False(first.Equals(null));
    }

    [Fact]
    public void ValueEquality_UsesTypeAndComponentsAcrossObjectTypedAndOperatorComparisons()
    {
        ValueObject first = new Value("same");
        ValueObject same = new Value("same");

        Assert.True(first.Equals((object)same));
        Assert.True(((IEquatable<ValueObject>)first).Equals(same));
        Assert.True(first == same);
        Assert.Equal(first.GetHashCode(), same.GetHashCode());
        Assert.False(first == new Value("different"));
        Assert.False(first.Equals(null));
    }

    private sealed class Entity : BaseEntity
    {
        public Entity(Guid id) => Id = id;
    }

    private sealed class OtherEntity : BaseEntity
    {
        public OtherEntity(Guid id) => Id = id;
    }

    private sealed class Value(string text) : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents() => [text];
    }
}
