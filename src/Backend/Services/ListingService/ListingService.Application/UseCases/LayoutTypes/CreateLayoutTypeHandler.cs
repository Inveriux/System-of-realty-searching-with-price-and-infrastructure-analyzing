using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.LayoutTypes;

public sealed record CreateLayoutTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateLayoutTypeHandler(ILookupHandler<LayoutType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateLayoutTypeCommand, Result>
{
    public async Task<Result> Handle(CreateLayoutTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
