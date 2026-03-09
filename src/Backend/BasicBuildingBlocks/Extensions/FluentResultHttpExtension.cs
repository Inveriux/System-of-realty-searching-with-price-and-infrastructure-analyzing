using BasicBuildingBlocks.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public static class FluentResultHttpExtension
{
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Result result)
    {
        if(result.IsSuccess)
            return Results.Ok();

        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage))
        };

        return Results.Problem(problem.Detail, statusCode: problem.Status, title: problem.Title);
    }

    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<TValue>(this Result<TValue> result)
    {
        if(result.IsSuccess)
            return Results.Ok(result);

        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage))
        };

        return Results.Problem(problem.Detail, statusCode: problem.Status, title: problem.Title);
    }
}