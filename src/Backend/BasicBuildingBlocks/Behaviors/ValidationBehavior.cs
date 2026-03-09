using BasicBuildingBlocks.Results;
using FluentValidation;
using MediatR;

namespace BasicBuildingBlocks.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull where TResponse : IResult
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

        throw new ValidationException("Validation failure", errors);
    }
}