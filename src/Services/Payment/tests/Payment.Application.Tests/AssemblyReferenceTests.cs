using System.Reflection;
using Xunit;

namespace Payment.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Payment.Application");

        Assert.Equal("Payment.Application", assembly.GetName().Name);
    }
}

