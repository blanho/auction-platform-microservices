using System.Reflection;
using Xunit;

namespace Catalog.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Catalog.Domain");

        Assert.Equal("Catalog.Domain", assembly.GetName().Name);
    }
}

