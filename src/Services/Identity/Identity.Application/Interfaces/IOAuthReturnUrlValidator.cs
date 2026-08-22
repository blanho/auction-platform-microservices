namespace Identity.Application.Interfaces;

public interface IOAuthReturnUrlValidator
{
    bool TryResolve(string? requestedReturnUrl, out string safeReturnUrl);
}
