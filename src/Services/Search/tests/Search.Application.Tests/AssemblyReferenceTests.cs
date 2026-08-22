using System.Reflection;
using Xunit;

namespace Search.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Search.Application");

        Assert.Equal("Search.Application", assembly.GetName().Name);
    }
}

