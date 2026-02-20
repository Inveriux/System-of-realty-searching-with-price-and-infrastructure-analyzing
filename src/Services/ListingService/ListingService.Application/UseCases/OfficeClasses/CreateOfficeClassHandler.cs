using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.OfficeClasses;

public sealed record CreateOfficeClassCommand(string ClassName) : IRequest<Result>;

public sealed class CreateOfficeClassHandler(ILookupHandler<OfficeClass, LookupClassNameDto> lookupHandler)
    : IRequestHandler<CreateOfficeClassCommand, Result>
{
    public async Task<Result> Handle(CreateOfficeClassCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.ClassName, cancellationToken);
}
