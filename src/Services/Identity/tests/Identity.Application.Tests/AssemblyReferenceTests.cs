using System.Reflection;
using Xunit;

namespace Identity.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Identity.Application");

        Assert.Equal("Identity.Application", assembly.GetName().Name);
    }
}

