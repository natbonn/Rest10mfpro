using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Exceptions;

namespace SchoolApp.Helpers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    // Logging
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var (statusCode, title, isExpected) = MapException(exception);
        if (isExpected)
        {
            _logger.LogWarning(exception, "{ExceptionType} at {Method} {Endpoint} by {User} {TraceId}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.User.Identity?.Name ?? "Anonymous",
                httpContext.TraceIdentifier
                );
        }
        else
        {
            _logger.LogError(exception, "Unhandled exception at {Method} {Endpoint} by {User} {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.User.Identity?.Name ?? "Anonymous",
                httpContext.TraceIdentifier
                );
        }

        // Response
        // RFC 7807 (Problem Details) API Contract for Error Responses .net & springboot (RFC: Request For Comments)
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = isExpected
                ? exception.Message
                : "An unexpected error occured.",
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.io/{statusCode}"
        };

        problemDetails.Extensions["TraceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "Application/problem+json",
            cancellationToken: cancellationToken
            );

        return true;
    }

    // Mapping
    private static (int StatusCode, string Title, bool IsExpected) MapException(Exception ex) => ex switch
    {
        EntityAlreadyExistsException => (StatusCodes.Status409Conflict, "Resource already exists", true),
        EntityNotFoundException => (StatusCodes.Status404NotFound, "Resource not found", true),
        InvalidArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument", true),
        EntityNotAuthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", true),
        EntityForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", true),
        _ => (StatusCodes.Status500InternalServerError, "Internal server error", false)
    };
}
