using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.Listings;

public sealed record AddListingMediaUrlsCommand(Guid ListingId, IReadOnlyCollection<string> MediaUrls) : IRequest<Result>;

public sealed class AddListingMediaUrlsHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<AddListingMediaUrlsCommand, Result>
{
    public async Task<Result> Handle(AddListingMediaUrlsCommand request, CancellationToken cancellationToken)
    {
        if (request.MediaUrls.Count == 0)
            return Result.Validation("At least one media url is required.");

        var listing = await repositoryManager.Listing.GetDetailedByIdAsync(request.ListingId, true, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound(
                "Listing not found.",
                [new ResultError("Listing not found.", 0, nameof(request.ListingId))]);
        }

        listing.Media ??= [];
        var hasMainMedia = listing.Media.Any(m => m.IsMainMedia);

        foreach (var mediaUrl in request.MediaUrls.Where(static url => !string.IsNullOrWhiteSpace(url)).Distinct())
        {
            var isMainMedia = !hasMainMedia;
            listing.Media.Add(new ListingMedia
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                Url = mediaUrl,
                IsMainMedia = isMainMedia
            });

            if (isMainMedia)
                hasMainMedia = true;
        }

        await repositoryManager.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
