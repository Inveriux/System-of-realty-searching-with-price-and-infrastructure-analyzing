using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.SewerTypes;

public sealed record CreateSewerTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateSewerTypeHandler(ILookupHandler<SewerType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateSewerTypeCommand, Result>
{
    public async Task<Result> Handle(CreateSewerTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
