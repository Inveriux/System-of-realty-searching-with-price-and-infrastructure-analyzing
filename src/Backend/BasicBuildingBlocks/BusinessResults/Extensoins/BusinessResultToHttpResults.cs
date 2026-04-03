using Microsoft.AspNetCore.Http;

namespace BasicBuildingBlocks.BusinessResults.Extensoins;

public static class BusinessResultToHttpResults
{
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Result result)
    {
        if(result.IsSuccess)
            return Results.Ok();

        var problem = CreateProblem(result.StatusCode, result.Message, result.Errors);
        return Results.Json(problem, statusCode: problem.Status);
    }

    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<TValue>(this Result<TValue> result)
    {
        if(result.IsSuccess)
            return Results.Ok(result);

        var problem = CreateProblem(result.StatusCode, result.Message, result.Errors);
        return Results.Json(problem, statusCode: problem.Status);
    }

    private static ApiProblemDetails CreateProblem(ResultStatusCode statusCode, string? message, IReadOnlyList<ResultError> errors)
        => new()
        {
            Title = GetTitle(statusCode),
            Status = GetHttpStatusCode(statusCode),
            Detail = message,
            Errors = errors.Select(e => new ApiErrorItem(e.ErrorMessage, e.Code, e.PropertyName)).ToList()
        };

    private static int GetHttpStatusCode(ResultStatusCode statusCode) => statusCode switch
    {
        ResultStatusCode.Success => StatusCodes.Status200OK,
        ResultStatusCode.Validation => StatusCodes.Status400BadRequest,
        ResultStatusCode.Conflict => StatusCodes.Status409Conflict,
        ResultStatusCode.NotFound => StatusCodes.Status404NotFound,
        ResultStatusCode.Unauthorized => StatusCodes.Status401Unauthorized,
        ResultStatusCode.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest
    };

    private static string GetTitle(ResultStatusCode statusCode) => statusCode switch
    {
        ResultStatusCode.Validation => "Ошибка валидации",
        ResultStatusCode.Conflict => "Конфликт данных",
        ResultStatusCode.NotFound => "Не найдено",
        ResultStatusCode.Unauthorized => "Требуется авторизация",
        ResultStatusCode.Forbidden => "Доступ запрещен",
        _ => "Ошибка запроса"
    };
}
