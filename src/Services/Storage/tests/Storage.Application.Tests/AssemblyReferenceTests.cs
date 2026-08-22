using System.Reflection;
using Xunit;

namespace Storage.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Storage.Application");

        Assert.Equal("Storage.Application", assembly.GetName().Name);
    }
}

