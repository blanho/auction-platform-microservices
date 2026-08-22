using System.Reflection;
using Xunit;

namespace Bidding.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Bidding.Domain");

        Assert.Equal("Bidding.Domain", assembly.GetName().Name);
    }
}

