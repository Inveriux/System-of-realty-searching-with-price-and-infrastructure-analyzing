using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Models;
using FluentResults;
using AutoMapper;
using MediatR;

namespace ListingService.Application.UseCases.Listings;

public record CreateListingCommand(CreateListingDto ListingDto) : IRequest<Result>;

public class CreateListingHandler(IRepositoryManager manager, IMapper mapper)
    : IRequestHandler<CreateListingCommand, Result>
{
    public async Task<Result> Handle(CreateListingCommand request, 
        CancellationToken cancellationToken)
    {   
        var listing = mapper.Map<Listing>(request.ListingDto);
        await manager.Listing.AddAsync(listing, cancellationToken);
        await manager.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}