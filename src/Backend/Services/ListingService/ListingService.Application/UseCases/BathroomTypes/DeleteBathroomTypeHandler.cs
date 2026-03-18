using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.BathroomTypes;

public sealed record DeleteBathroomTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteBathroomTypeHandler(ILookupHandler<BathroomType, LookupNameDto> lookupHandler) 
    : IRequestHandler<DeleteBathroomTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteBathroomTypeCommand request, 
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
