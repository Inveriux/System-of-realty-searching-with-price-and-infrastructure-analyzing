using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RenovationTypes;

public sealed record CreateRenovationTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateRenovationTypeHandler(ILookupHandler<RenovationType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateRenovationTypeCommand, Result>
{
    public async Task<Result> Handle(CreateRenovationTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
