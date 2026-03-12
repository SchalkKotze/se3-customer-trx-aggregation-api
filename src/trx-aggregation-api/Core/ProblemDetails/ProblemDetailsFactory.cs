using Microsoft.AspNetCore.Mvc;

namespace aggregate_api.Core.ProblemDetails;

/// <summary>
/// Factory for creating standardized HTTP Problem Details responses according to RFC 7807.
/// Problem Details provides a standard way to describe HTTP errors in machine-readable format.
/// </summary>
public static class ProblemDetailsFactory
{
    /// <summary>
    /// Creates a Problem Details response for bad request (400) errors.
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateBadRequestProblem(
        string detail,
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        return CreateProblem(
            status: StatusCodes.Status400BadRequest,
            title: "Bad Request",
            detail: detail,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            instance: instance,
            extensions: extensions
        );
    }

    /// <summary>
    /// Creates a Problem Details response for unauthorized (401) errors.
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnauthorizedProblem(
        string detail = "The request requires authentication.",
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        return CreateProblem(
            status: StatusCodes.Status401Unauthorized,
            title: "Unauthorized",
            detail: detail,
            type: "https://tools.ietf.org/html/rfc7235#section-3.1",
            instance: instance,
            extensions: extensions
        );
    }

    /// <summary>
    /// Creates a Problem Details response for forbidden (403) errors.
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateForbiddenProblem(
        string detail = "You do not have permission to access this resource.",
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        return CreateProblem(
            status: StatusCodes.Status403Forbidden,
            title: "Forbidden",
            detail: detail,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            instance: instance,
            extensions: extensions
        );
    }

    /// <summary>
    /// Creates a Problem Details response for not found (404) errors.
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateNotFoundProblem(
        string detail,
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        return CreateProblem(
            status: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: detail,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            instance: instance,
            extensions: extensions
        );
    }

    /// <summary>
    /// Creates a Problem Details response for validation errors (422).
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateValidationErrorsProblem(
        Dictionary<string, string[]> errors,
        string? instance = null)
    {
        var extensions = new Dictionary<string, object> { { "errors", errors } };

        return CreateProblem(
            status: StatusCodes.Status422UnprocessableEntity,
            title: "One or more validation errors occurred.",
            detail: "See the errors property for details.",
            type: "https://tools.ietf.org/html/rfc4918#section-11.2",
            instance: instance,
            extensions: extensions
        );
    }

    /// <summary>
    /// Creates a Problem Details response for internal server errors (500).
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateInternalServerErrorProblem(
        string detail = "An internal server error occurred.",
        string? instance = null,
        Dictionary<string, object>? extensions = null,
        string? traceId = null)
    {
        var allExtensions = extensions ?? new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(traceId))
        {
            allExtensions["traceId"] = traceId;
        }

        return CreateProblem(
            status: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error",
            detail: detail,
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            instance: instance,
            extensions: allExtensions
        );
    }

    /// <summary>
    /// Creates a Problem Details response for service unavailable (503) errors.
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateServiceUnavailableProblem(
        string detail = "The service is temporarily unavailable.",
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        return CreateProblem(
            status: StatusCodes.Status503ServiceUnavailable,
            title: "Service Unavailable",
            detail: detail,
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.3",
            instance: instance,
            extensions: extensions
        );
    }

    /// <summary>
    /// Creates a generic Problem Details response.
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateProblem(
        int status,
        string title,
        string detail,
        string type,
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = instance ?? string.Empty
        };

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return problemDetails;
    }
}
