using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ClassTypes;

public sealed record CreateClassTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateClassTypeHandler(ILookupHandler<ClassType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateClassTypeCommand, Result>
{
    public async Task<Result> Handle(CreateClassTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
