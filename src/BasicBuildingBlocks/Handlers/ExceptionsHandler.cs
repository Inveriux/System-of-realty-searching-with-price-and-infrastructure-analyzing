using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        await httpContext.Response.WriteAsJsonAsync(result);
        return true;
    }

    public ProblemDetails HandleValidationException(ValidationException exception)
        => new()
        {
            Title = exception.Message,
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join(",", exception.Errors.ToDictionary(e => e.PropertyName, 
                                                                    e => e.ErrorMessage))
        };

    public ProblemDetails HandleUndefindedExcetipn(Exception exception)
        => new()
        {
            Title = "Undefinded fauilure",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message + "\n" + exception.InnerException?.Message
        };
}