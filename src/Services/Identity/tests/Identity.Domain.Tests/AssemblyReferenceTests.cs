using System.Reflection;
using Xunit;

namespace Identity.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Identity.Domain");

        Assert.Equal("Identity.Domain", assembly.GetName().Name);
    }
}

