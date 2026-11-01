using System.Reflection;

namespace Auction.Application.Tests;

public class InterfaceStub : DispatchProxy
{
    public Func<MethodInfo, object?[], object?> InvokeHandler { get; set; } = null!;

    public static T For<T>(Func<MethodInfo, object?[], object?> handler) where T : class
    {
        var proxy = Create<T, InterfaceStub>();
        ((InterfaceStub)(object)proxy).InvokeHandler = handler;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
        InvokeHandler(targetMethod!, args!);
}
