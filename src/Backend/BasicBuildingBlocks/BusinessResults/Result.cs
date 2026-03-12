namespace BasicBuildingBlocks.BusinessResults;

public enum ResultStatusCode
{
    Success = 0,
    Fail = 1,
    Validation = 2,
    Conflict = 3,
    Undefined = 4,
    NotFound = 5,
    Unauthorized = 6,
    Forbidden = 7
}

public interface IResultFactory<TSelf>
    where TSelf : IResultFactory<TSelf>
{
    static abstract TSelf Fail(string? message, IEnumerable<ResultError>? errors = null);
    static abstract TSelf Fail(ResultStatusCode statusCode, string? message, IEnumerable<ResultError>? errors = null);
}

public record ResultError(string ErrorMessage, int? Code = null, string? PropertyName = null);

public interface IResult
{
    bool IsSuccess { get; }
    string? Message { get; }
    IReadOnlyList<ResultError> Errors { get; }
    ResultStatusCode StatusCode { get; }
}

public interface IResult<out T> : IResult
{
    T? Data { get; }
}

public class Result : IResult, IResultFactory<Result>
{
    private readonly List<ResultError> _errors = [];
    public bool IsSuccess { get; private init; }
    public string? Message { get; private init; }
    public ResultStatusCode StatusCode { get; private init; } = ResultStatusCode.Undefined;
    public IReadOnlyList<ResultError> Errors => _errors;

    private Result() { }

    public static Result Ok(string? message = null)
        => new()
        {
            IsSuccess = true,
            Message = message,
            StatusCode = ResultStatusCode.Success
        };

    public static Result Fail(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Fail, message, errors);

    public static Result Fail(ResultStatusCode statusCode, string? message = null, IEnumerable<ResultError>? errors = null)
    {
        var r = new Result
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode
        };

        r._errors.AddRange(errors ?? []);

        return r;
    }

    public static Result Validation(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Validation, message, errors);

    public static Result NotFound(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.NotFound, message, errors);

    public static Result Conflict(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Conflict, message, errors);

    public static Result Unauthorized(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Unauthorized, message, errors);

    public static Result Forbidden(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Forbidden, message, errors);
    
    public static Result<T> Ok<T>(T data, string? message = null)
        => Result<T>.Ok(data, message);

    public static Result<T> Fail<T>(string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.Fail(message, errors);

    public static Result<T> Fail<T>(ResultStatusCode statusCode, string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.Fail(statusCode, message, errors);

    public static Result<T> Validation<T>(string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.Validation(message, errors);

    public static Result<T> NotFound<T>(string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.NotFound(message, errors);

    public static Result<T> Conflict<T>(string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.Conflict(message, errors);

    public static Result<T> Unauthorized<T>(string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.Unauthorized(message, errors);

    public static Result<T> Forbidden<T>(string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.Forbidden(message, errors);

    public Result WithError(string errorMessage, int? code = null, string? propertyName = null)
    {
        _errors.Add(new ResultError(errorMessage, code, propertyName));
        return this;
    }

    public Result WithErrors(IEnumerable<ResultError> errors)
    {
        _errors.AddRange(errors);
        return this;
    }
}

public class Result<T> : IResult<T>, IResultFactory<Result<T>>
{
    private readonly List<ResultError> _errors = [];
    public bool IsSuccess { get; private init; }
    public string? Message { get; private init; }
    public ResultStatusCode StatusCode { get; private init; } = ResultStatusCode.Undefined;

    public T? Data { get; private init; }
    public IReadOnlyList<ResultError> Errors => _errors;

    private Result() { }

    public static Result<T> Ok(T data, string? message = null)
        => new()
        {
            IsSuccess = true,
            Message = message,
            Data = data,
            StatusCode = ResultStatusCode.Success
        };

    public static Result<T> Fail(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Fail, message, errors);

    public static Result<T> Fail(ResultStatusCode statusCode, string? message = null, IEnumerable<ResultError>? errors = null)
    {
        var r = new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            StatusCode = statusCode
        };

        r._errors.AddRange(errors ?? []);

        return r;
    }

    public static Result<T> Validation(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Validation, message, errors);

    public static Result<T> NotFound(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.NotFound, message, errors);

    public static Result<T> Conflict(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Conflict, message, errors);

    public static Result<T> Unauthorized(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Unauthorized, message, errors);

    public static Result<T> Forbidden(string? message = null, IEnumerable<ResultError>? errors = null)
        => Fail(ResultStatusCode.Forbidden, message, errors);

    public Result<T> WithError(string errorMessage, int? code = null, string? propertyName = null)
    {
        _errors.Add(new ResultError(errorMessage, code, propertyName));
        return this;
    }

    public Result<T> WithErrors(IEnumerable<ResultError> errors)
    {
        _errors.AddRange(errors);
        return this;
    }

    public Result<T> WithErrors(IEnumerable<string> errorMessages)
    {
        _errors.AddRange(errorMessages.Select(em => new ResultError(em)));
        return this;
    }
}
