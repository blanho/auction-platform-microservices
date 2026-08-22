using System.Reflection;
using Xunit;

namespace Auction.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Auction.Application");

        Assert.Equal("Auction.Application", assembly.GetName().Name);
    }
}

