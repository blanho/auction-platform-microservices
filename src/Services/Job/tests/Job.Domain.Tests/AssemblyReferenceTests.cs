using System.Reflection;
using Xunit;

namespace Job.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Job.Domain");

        Assert.Equal("Job.Domain", assembly.GetName().Name);
    }
}

