using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.OfficeClasses;

public sealed record CreateOfficeClassCommand(string Name) : IRequest<Result>;

public sealed class CreateOfficeClassHandler(ILookupHandler<OfficeClass, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateOfficeClassCommand, Result>
{
    public async Task<Result> Handle(CreateOfficeClassCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
