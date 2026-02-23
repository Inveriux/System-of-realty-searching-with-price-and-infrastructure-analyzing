using FluentValidation;
using FluentResults;
using MediatR;

namespace BasicBuildingBlocks.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) 
    : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : IRequest<Result<TResponse>>
{
    public async Task<Result<TResponse>> Handle(TRequest request, 
        RequestHandlerDelegate<Result<TResponse>> next, 
        CancellationToken cancellationToken)
    {
        var validationContext = new ValidationContext<TRequest>(request);
        
        var validationResult = await Task.WhenAll(validators
            .Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        var errors = validationResult.SelectMany(v => v.Errors).ToList();
        
        if(!errors.Any())
            return await next();

        var result = Result.Fail<TResponse>("Validation failure");
        errors.ForEach(e => result.WithError(e.ErrorMessage));

        return result;
    }
}