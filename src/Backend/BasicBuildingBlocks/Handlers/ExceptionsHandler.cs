using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using BasicBuildingBlocks.BusinessResults;
using FluentValidation;

namespace BasicBuildingBlocks.Handlers;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var result = exception switch
        {
            ValidationException validationException => HandleValidationException(validationException),
            _ => HandleUndefindedExcetipn(exception)
        };

        httpContext.Response.StatusCode = result.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(result);
        return true;
    }

    public ApiProblemDetails HandleValidationException(ValidationException exception)
        => new()
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            Errors = exception.Errors
                .Select(e => new ApiErrorItem(e.ErrorMessage, null, e.PropertyName))
                .ToList()
        };

    public ApiProblemDetails HandleUndefindedExcetipn(Exception exception)
        => new()
        {
            Title = "Undefinded fauilure",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Unexpected server error.",
            Errors =
            [
                new ApiErrorItem(exception.Message + "\n" + exception.InnerException?.Message)
            ]
        };
}
