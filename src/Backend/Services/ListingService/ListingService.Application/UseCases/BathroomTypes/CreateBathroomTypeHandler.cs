using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models; 
using FluentResults;
using MediatR;


namespace ListingService.Application.UseCases.BathroomTypes;

public sealed record CreateBathroomTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateBathroomTypeHandler(ILookupHandler<BathroomType, LookupNameDto> lookupHandler) 
    : IRequestHandler<CreateBathroomTypeCommand, Result>
{
    public async Task<Result> Handle(CreateBathroomTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}