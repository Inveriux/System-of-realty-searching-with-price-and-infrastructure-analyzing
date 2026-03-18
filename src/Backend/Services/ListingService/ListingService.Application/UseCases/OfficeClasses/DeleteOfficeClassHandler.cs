using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.OfficeClasses;

public sealed record DeleteOfficeClassCommand(string Name) : IRequest<Result>;

public sealed class DeleteOfficeClassHandler(ILookupHandler<OfficeClass, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteOfficeClassCommand, Result>
{
    public async Task<Result> Handle(DeleteOfficeClassCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
