using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ClassTypes;

public sealed record DeleteClassTypeCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteClassTypeHandler(ILookupHandler<ClassType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteClassTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteClassTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
