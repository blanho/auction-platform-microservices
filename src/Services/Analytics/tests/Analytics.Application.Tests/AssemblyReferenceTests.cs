using System.Reflection;
using Xunit;

namespace Analytics.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Analytics.Application");

        Assert.Equal("Analytics.Application", assembly.GetName().Name);
    }
}

