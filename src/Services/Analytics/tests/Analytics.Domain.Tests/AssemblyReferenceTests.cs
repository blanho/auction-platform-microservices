using System.Reflection;
using Xunit;

namespace Analytics.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Analytics.Domain");

        Assert.Equal("Analytics.Domain", assembly.GetName().Name);
    }
}

