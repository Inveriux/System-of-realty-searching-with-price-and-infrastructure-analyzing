using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.SewerTypes;

public sealed record CreateSewerTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateSewerTypeHandler(ILookupHandler<SewerType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateSewerTypeCommand, Result>
{
    public async Task<Result> Handle(CreateSewerTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
