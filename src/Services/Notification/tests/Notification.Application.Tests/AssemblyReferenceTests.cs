using System.Reflection;
using Xunit;

namespace Notification.Application.Tests;

public sealed class AssemblyReferenceTests
{
    [Fact]
    public void TargetAssembly_IsLoadable()
    {
        var assembly = Assembly.Load("Notification.Application");

        Assert.Equal("Notification.Application", assembly.GetName().Name);
    }
}

