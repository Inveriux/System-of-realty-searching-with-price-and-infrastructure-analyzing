using ListingService.Application.Contracts.Services;
using ListingService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ListingService.Application.UseCases.Listings;

public sealed record HideListingCommand(Guid Id) : IRequest<Result>;

public sealed class HideListingHandler(
    IRepositoryManager repositoryManager,
    ISearchServiceClient searchServiceClient,
    ILogger<HideListingHandler> logger)
    : IRequestHandler<HideListingCommand, Result>
{
    public async Task<Result> Handle(HideListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repositoryManager.Listing.GetByIdAsync(request.Id, true, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound(
                "Listing not found.",
                [new ResultError("Listing not found.", 0, nameof(request.Id))]);
        }

        if (string.Equals(listing.Status, ListingStatus.Hidden, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Ok("Listing is already hidden.");
        }

        listing.Status = ListingStatus.Hidden;
        await repositoryManager.SaveChangesAsync(cancellationToken);

        var deletedFromIndex = await searchServiceClient.DeleteListingAsync(request.Id, cancellationToken);
        if (!deletedFromIndex)
        {
            logger.LogWarning(
                "Listing {ListingId} was hidden, but removing it from search index failed.",
                request.Id);
        }

        return Result.Ok();
    }
}
