using System.Reflection;
using Xunit;

namespace Notification.Domain.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Notification.Domain");

        Assert.Equal("Notification.Domain", assembly.GetName().Name);
    }
}

