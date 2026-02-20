using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BathroomTypes;

public sealed record DeleteBathroomTypeCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteBathroomTypeHandler(ILookupHandler<BathroomType, LookupTypeNameDto> lookupHandler) 
    : IRequestHandler<DeleteBathroomTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteBathroomTypeCommand request, 
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}