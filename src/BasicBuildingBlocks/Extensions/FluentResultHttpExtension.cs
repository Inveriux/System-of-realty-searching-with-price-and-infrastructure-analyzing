using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentResults;

public static class FluentResultHttpExtension
{
    public static IResult ToHttpResult(this Result result)
    {
        if(result.IsSuccess)
            return Results.Ok();

        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
        };

        return Results.Problem(problem.Detail, statusCode: problem.Status, title: problem.Title);
    }

    public static IResult ToHttpResult<TValue>(this Result<TValue> result)
    {
        if(result.IsSuccess)
            return Results.Ok(result.Value);

        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
        };

        return Results.Problem(problem.Detail, statusCode: problem.Status, title: problem.Title);
    }
}