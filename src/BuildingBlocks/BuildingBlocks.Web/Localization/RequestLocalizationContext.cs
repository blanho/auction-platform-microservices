using System.Threading;
using BuildingBlocks.Application.Localization;

namespace BuildingBlocks.Web.Localization;

internal static class RequestLocalizationContext
{
    private static readonly AsyncLocal<ILocalizationService?> CurrentLocalizer = new();

    public static ILocalizationService? Current => CurrentLocalizer.Value;

    public static IDisposable Push(ILocalizationService localizer)
    {
        var previous = CurrentLocalizer.Value;
        CurrentLocalizer.Value = localizer;
        return new Scope(previous);
    }

    private sealed class Scope(ILocalizationService? previous) : IDisposable
    {
        public void Dispose() => CurrentLocalizer.Value = previous;
    }
}
