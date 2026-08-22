using System.Reflection;
using Xunit;

namespace Storage.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Storage.Domain");

        Assert.Equal("Storage.Domain", assembly.GetName().Name);
    }
}

