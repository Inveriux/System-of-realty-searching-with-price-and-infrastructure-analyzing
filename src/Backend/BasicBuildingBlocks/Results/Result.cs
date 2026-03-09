namespace BasicBuildingBlocks.Results;

public enum ResultStatusCode
{
    Success = 0,
    Fail = 1,
    Undefined = 2
}

public record ResultError(string ErrorMessage, int? Code = null, string? PropertyName = null);

public interface IResult
{
    bool IsSuccess { get; }
    string? Message { get; }
    IReadOnlyList<ResultError> Errors { get; }
}

public interface IResult<out T> : IResult
{
    T? Data { get; }
}

public class Result : IResult
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
    {
        var r = new Result
        {
            IsSuccess = false,
            Message = message,
            StatusCode = ResultStatusCode.Fail
        };

        r._errors.AddRange(errors ?? []);

        return r;
    }
    
    public static Result<T> Ok<T>(T data, string? message = null)
        => Result<T>.Ok(data, message);

    public static Result<T> Fail<T>(string? message = null, IEnumerable<ResultError>? errors = null)
        => Result<T>.Fail(message, errors);

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

public class Result<T> : IResult<T>
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
    {
        var r = new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            StatusCode = ResultStatusCode.Fail
        };

        r._errors.AddRange(errors ?? []);

        return r;
    }

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