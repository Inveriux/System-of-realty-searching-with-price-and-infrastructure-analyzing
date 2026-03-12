using BasicBuildingBlocks.BusinessResults;
using FluentValidation;
using MediatR;

namespace BasicBuildingBlocks.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull 
    where TResponse : IResult, IResultFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var validationContext = new ValidationContext<TRequest>(request);
        
        var validationResult = await Task.WhenAll(validators
            .Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        var errors = validationResult.SelectMany(v => v.Errors).ToList();
        
        if(!errors.Any())
            return await next();

        return TResponse.Fail(ResultStatusCode.Validation, "Validation failed.", 
            errors.Select(e => new ResultError(e.ErrorMessage, null, NormalizePropertyName(e.PropertyName))));
    }

    private static string? NormalizePropertyName(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return propertyName;

        var segments = propertyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 ? segments[^1] : propertyName;
    }
}
