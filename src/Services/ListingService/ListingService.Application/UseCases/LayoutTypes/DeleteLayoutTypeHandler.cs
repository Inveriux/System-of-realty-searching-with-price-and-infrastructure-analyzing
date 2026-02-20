using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LayoutTypes;

public sealed record DeleteLayoutTypeCommand(string ClassName) : IRequest<Result>;

public sealed class DeleteLayoutTypeHandler(ILookupHandler<LayoutType, LookupClassNameDto> lookupHandler)
    : IRequestHandler<DeleteLayoutTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteLayoutTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.ClassName, cancellationToken);
}
