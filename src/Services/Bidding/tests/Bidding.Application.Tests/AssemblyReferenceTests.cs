using System.Reflection;
using Xunit;

namespace Bidding.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Bidding.Application");

        Assert.Equal("Bidding.Application", assembly.GetName().Name);
    }
}

