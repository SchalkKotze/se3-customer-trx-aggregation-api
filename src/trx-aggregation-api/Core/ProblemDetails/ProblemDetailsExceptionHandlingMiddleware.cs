using System.Diagnostics;
using System.Text.Json;
using aggregate_api.Infrastructure.Contracts;

namespace aggregate_api.Core.ProblemDetails;

/// <summary>
/// Global exception handling middleware that returns HTTP Problem Details responses.
/// This middleware catches all unhandled exceptions and converts them to appropriate Problem Details responses.
/// </summary>
public class ProblemDetailsExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggingService _loggingService;

    public ProblemDetailsExceptionHandlingMiddleware(RequestDelegate next, ILoggingService loggingService)
    {
        _next = next;
        _loggingService = loggingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException ex)
        {
            _loggingService.LogWarning($"Request cancelled: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            await WriteProblemDetailsAsync(
                context,
                ProblemDetailsFactory.CreateProblem(
                    status: StatusCodes.Status499ClientClosedRequest,
                    title: "Client Closed Request",
                    detail: "The client closed the request.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    instance: context.Request.Path
                )
            );
        }
        catch (ArgumentException ex)
        {
            _loggingService.LogWarning($"Invalid argument: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteProblemDetailsAsync(
                context,
                ProblemDetailsFactory.CreateBadRequestProblem(
                    detail: ex.Message,
                    instance: context.Request.Path
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            _loggingService.LogWarning($"Invalid operation: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteProblemDetailsAsync(
                context,
                ProblemDetailsFactory.CreateBadRequestProblem(
                    detail: ex.Message,
                    instance: context.Request.Path
                )
            );
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            _loggingService.LogError($"Unhandled exception occurred. TraceId: {traceId}", ex);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteProblemDetailsAsync(
                context,
                ProblemDetailsFactory.CreateInternalServerErrorProblem(
                    detail: "An unexpected error occurred while processing your request.",
                    instance: context.Request.Path,
                    traceId: traceId
                )
            );
        }
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}

/// <summary>
/// Extension method to add the Problem Details exception handling middleware to the pipeline.
/// </summary>
public static class ProblemDetailsExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseProblemDetailsExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ProblemDetailsExceptionHandlingMiddleware>();
        return app;
    }
}
