using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models; 
using FluentResults;
using MediatR;


namespace ListingService.Application.UseCases.BathroomTypes;

public sealed record CreateBathroomTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateBathroomTypeHandler(ILookupHandler<BathroomType, LookupTypeNameDto> lookupHandler) 
    : IRequestHandler<CreateBathroomTypeCommand, Result>
{
    public async Task<Result> Handle(CreateBathroomTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}