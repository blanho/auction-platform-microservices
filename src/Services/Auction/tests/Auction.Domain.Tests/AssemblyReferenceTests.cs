using System.Reflection;
using Xunit;

namespace Auction.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Auction.Domain");

        Assert.Equal("Auction.Domain", assembly.GetName().Name);
    }
}

