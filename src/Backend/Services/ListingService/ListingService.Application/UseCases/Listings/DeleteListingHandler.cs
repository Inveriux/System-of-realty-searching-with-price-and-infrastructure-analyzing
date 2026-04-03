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
            var removedFromSearchIndex = await searchServiceClient.DeleteListingAsync(request.Id, cancellationToken);
            if (removedFromSearchIndex)
            {
                logger.LogInformation(
                    "Listing {ListingId} was not found in ListingService, but stale search index entry was removed.",
                    request.Id);

                return Result.Ok("Объявление уже было удалено из основного хранилища. Устаревшая запись из поиска удалена.");
            }

            return Result.NotFound(
                "Объявление не найдено.",
                [new ResultError("Объявление не найдено.", 0, nameof(request.Id))]);
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
