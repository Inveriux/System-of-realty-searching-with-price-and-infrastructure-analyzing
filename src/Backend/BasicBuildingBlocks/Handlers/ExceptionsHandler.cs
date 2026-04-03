using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using BasicBuildingBlocks.BusinessResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BasicBuildingBlocks.Handlers;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            logger.LogWarning(validationException,
                "Validation exception for {Method} {Path}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                validationException.Message);
        }
        else
        {
            logger.LogError(exception,
                "Unhandled exception for {Method} {Path}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }

        var result = exception switch
        {
            ValidationException ex => HandleValidationException(ex),
            _ => HandleUndefindedExcetipn(exception)
        };

        httpContext.Response.StatusCode = result.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);
        return true;
    }

    public ApiProblemDetails HandleValidationException(ValidationException exception)
        => new()
        {
            Title = "Ошибка валидации",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            Errors = exception.Errors
                .Select(e => new ApiErrorItem(e.ErrorMessage, null, e.PropertyName))
                .ToList()
        };

    public ApiProblemDetails HandleUndefindedExcetipn(Exception exception)
        => new()
        {
            Title = "Внутренняя ошибка",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Произошла непредвиденная ошибка сервера.",
            Errors =
            [
                new ApiErrorItem(exception.Message + "\n" + exception.InnerException?.Message)
            ]
        };
}
