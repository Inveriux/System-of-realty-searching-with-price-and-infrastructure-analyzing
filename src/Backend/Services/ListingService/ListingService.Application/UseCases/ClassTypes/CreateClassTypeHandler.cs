using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.ClassTypes;

public sealed record CreateClassTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateClassTypeHandler(ILookupHandler<ClassType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateClassTypeCommand, Result>
{
    public async Task<Result> Handle(CreateClassTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
