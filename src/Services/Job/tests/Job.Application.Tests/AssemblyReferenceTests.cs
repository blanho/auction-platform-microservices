using System.Reflection;
using Xunit;

namespace Job.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Job.Application");

        Assert.Equal("Job.Application", assembly.GetName().Name);
    }
}

