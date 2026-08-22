using System.Reflection;
using Xunit;

namespace Catalog.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Catalog.Application");

        Assert.Equal("Catalog.Application", assembly.GetName().Name);
    }
}

