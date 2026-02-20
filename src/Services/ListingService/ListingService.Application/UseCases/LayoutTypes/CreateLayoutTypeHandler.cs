using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LayoutTypes;

public sealed record CreateLayoutTypeCommand(string ClassName) : IRequest<Result>;

public sealed class CreateLayoutTypeHandler(ILookupHandler<LayoutType, LookupClassNameDto> lookupHandler)
    : IRequestHandler<CreateLayoutTypeCommand, Result>
{
    public async Task<Result> Handle(CreateLayoutTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.ClassName, cancellationToken);
}
