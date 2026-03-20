using ListingService.Application.Contracts.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ListingService.Application.UseCases.Listings;

public record DeleteListingCommand(Guid Id) : IRequest<Result>;

public class DeleteListingHandler(
    IRepositoryManager repositoryManager,
    ISearchServiceClient searchServiceClient,
    ILogger<DeleteListingHandler> logger)
    : IRequestHandler<DeleteListingCommand, Result>
{
    public async Task<Result> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repositoryManager.Listing.GetByIdAsync(request.Id, true, cancellationToken);

        if (listing is null)
        {
            return Result.NotFound(
                "Listing not found.",
                [new ResultError("Listing not found.", 0, nameof(request.Id))]);
        }

        await repositoryManager.Listing.DeleteAsync(request.Id, cancellationToken);
        await repositoryManager.SaveChangesAsync(cancellationToken);

        var deletedFromIndex = await searchServiceClient.DeleteListingAsync(request.Id, cancellationToken);
        if (!deletedFromIndex)
        {
            logger.LogWarning(
                "Listing {ListingId} was deleted, but removing it from search index failed.",
                request.Id);
        }

        return Result.Ok();
    }
}
