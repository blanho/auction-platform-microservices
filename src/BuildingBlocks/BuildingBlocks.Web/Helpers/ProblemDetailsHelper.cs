using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Web.Constants;
using BuildingBlocks.Web.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web.Helpers;

public static class ProblemDetailsHelper
{
    private const string DefaultErrorTypeBaseUrl = "https://api.example.com/errors";

    public static ProblemDetails FromError(Error error, string? errorTypeBaseUrl = null)
    {
        return FromError(error, RequestLocalizationContext.Current, errorTypeBaseUrl);
    }

    public static ProblemDetails FromError(Error error, ILocalizationService? localizer, string? errorTypeBaseUrl = null)
    {
        var baseUrl = errorTypeBaseUrl ?? DefaultErrorTypeBaseUrl;
        var detail = ResolveLocalizedMessage(error, localizer);

        var problemDetails = new ProblemDetails
        {
            Title = error.Code,
            Detail = detail,
            Status = GetStatusCode(error),
            Type = $"{baseUrl}/{error.Code.ToLowerInvariant().Replace('.', '-')}"
        };

        if (error is ValidationError validationError)
        {
            problemDetails.Extensions[ProblemDetailsExtensionKeys.Errors] =
                ValidationErrorLocalizer.Localize(validationError.Errors, localizer);
        }

        return problemDetails;
    }

    private static string ResolveLocalizedMessage(Error error, ILocalizationService? localizer)
    {
        if (localizer == null)
            return error.Message;

        if (error is LocalizableError localizableError && localizableError.Parameters.Length > 0)
        {
            var template = localizer.GetLocalizedString(error.Code);
            return template.ResourceNotFound
                ? error.Message
                : string.Format(template.Value, localizableError.Parameters);
        }

        var localized = localizer.GetLocalizedString(error.Code);
        return localized.ResourceNotFound ? error.Message : localized.Value;
    }

    public static ProblemDetails Create(
        string title,
        string detail,
        int statusCode,
        string? errorTypeBaseUrl = null)
    {
        var baseUrl = errorTypeBaseUrl ?? DefaultErrorTypeBaseUrl;
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Type = $"{baseUrl}/{title.ToLowerInvariant().Replace('.', '-')}"
        };
    }

    public static ProblemDetails ValidationError(string detail, string? errorTypeBaseUrl = null)
    {
        return Create("Validation.Error", detail, StatusCodes.Status400BadRequest, errorTypeBaseUrl);
    }

    public static ProblemDetails ValidationError(
        string detail,
        Dictionary<string, string[]> errors,
        string? errorTypeBaseUrl = null)
    {
        var localizer = RequestLocalizationContext.Current;
        var problemDetails = ValidationError(detail, errorTypeBaseUrl);
        problemDetails.Extensions[ProblemDetailsExtensionKeys.Errors] =
            ValidationErrorLocalizer.Localize(errors, localizer);
        return problemDetails;
    }

    public static ProblemDetails ValidationError(
        Dictionary<string, string[]> errors,
        string? errorTypeBaseUrl = null)
    {
        var localizer = RequestLocalizationContext.Current;
        var localizedErrors = ValidationErrorLocalizer.Localize(errors, localizer);
        var detail = localizer?.GetString(LocalizationKeys.Validation.Failed) ?? "Validation failed";
        var problemDetails = ValidationError(detail, errorTypeBaseUrl);
        problemDetails.Extensions[ProblemDetailsExtensionKeys.Errors] = localizedErrors;
        return problemDetails;
    }

    public static ProblemDetails ValidationError(
        string fieldName,
        string errorMessage,
        string? errorTypeBaseUrl = null)
    {
        return ValidationError(
            new Dictionary<string, string[]> { [fieldName] = [errorMessage] },
            errorTypeBaseUrl);
    }

    public static ProblemDetails ValidationError(
        List<string>? errors,
        string? errorTypeBaseUrl = null)
    {
        var errorList = errors ?? new List<string>();
        var localizer = RequestLocalizationContext.Current;
        var localizedErrors = errorList
            .Select(error => ValidationErrorLocalizer.LocalizeMessage(error, localizer))
            .ToList();
        var detail = localizer?.GetString(LocalizationKeys.Validation.Failed) ?? "Validation failed";
        var problemDetails = ValidationError(detail, errorTypeBaseUrl);
        if (errorList.Count > 0)
        {
            problemDetails.Extensions[ProblemDetailsExtensionKeys.Errors] = new Dictionary<string, string[]>
            {
                [ProblemDetailsExtensionKeys.General] = localizedErrors.ToArray()
            };
        }
        return problemDetails;
    }

    public static ProblemDetails NotFound(
        string entityName,
        object? id = null,
        string? errorTypeBaseUrl = null)
    {
        var detail = id != null
            ? $"{entityName} with ID {id} was not found"
            : $"{entityName} was not found";

        return Create($"{entityName}.NotFound", detail, StatusCodes.Status404NotFound, errorTypeBaseUrl);
    }

    public static ProblemDetails Unauthorized(
        string detail = "Authentication is required",
        string? errorTypeBaseUrl = null)
    {
        return Create("Authentication.Required", detail, StatusCodes.Status401Unauthorized, errorTypeBaseUrl);
    }

    public static ProblemDetails Forbidden(
        string detail = "You do not have permission to perform this action",
        string? errorTypeBaseUrl = null)
    {
        return Create("Authorization.Forbidden", detail, StatusCodes.Status403Forbidden, errorTypeBaseUrl);
    }

    public static ProblemDetails Conflict(
        string detail,
        string? errorTypeBaseUrl = null)
    {
        return Create("Resource.Conflict", detail, StatusCodes.Status409Conflict, errorTypeBaseUrl);
    }

    public static ProblemDetails InternalError(
        string detail = "An unexpected error occurred",
        string? errorTypeBaseUrl = null)
    {
        return Create("Server.InternalError", detail, StatusCodes.Status500InternalServerError, errorTypeBaseUrl);
    }

    private static int GetStatusCode(Error error)
    {
        return error.Code switch
        {
            var code when code.Contains("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Validation") => StatusCodes.Status400BadRequest,
            var code when code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            var code when code.Contains("Conflict") => StatusCodes.Status409Conflict,
            var code when code.Contains("InternalError") => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
