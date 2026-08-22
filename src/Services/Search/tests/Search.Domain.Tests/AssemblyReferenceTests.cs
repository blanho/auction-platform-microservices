using System.Reflection;
using Xunit;

namespace Search.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Search.Domain");

        Assert.Equal("Search.Domain", assembly.GetName().Name);
    }
}

