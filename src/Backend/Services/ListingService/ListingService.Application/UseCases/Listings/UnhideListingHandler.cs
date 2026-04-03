using ListingService.Application.Contracts.Services;
using ListingService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ListingService.Application.UseCases.Listings;

public sealed record UnhideListingCommand(Guid Id) : IRequest<Result>;

public sealed class UnhideListingHandler(
    IRepositoryManager repositoryManager,
    ISearchServiceClient searchServiceClient,
    ILogger<UnhideListingHandler> logger)
    : IRequestHandler<UnhideListingCommand, Result>
{
    public async Task<Result> Handle(UnhideListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repositoryManager.Listing.GetDetailedByIdAsync(request.Id, true, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound(
                "Объявление не найдено.",
                [new ResultError("Объявление не найдено.", 0, nameof(request.Id))]);
        }

        if (!string.Equals(listing.Status, ListingStatus.Hidden, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Ok("Объявление уже активно.");
        }

        listing.Status = ListingStatus.Active;
        await repositoryManager.SaveChangesAsync(cancellationToken);

        var indexed = await searchServiceClient.IndexListingAsync(new SearchListingIndexRequest
        {
            ListingId = listing.Id,
            AgentId = listing.AgentId,
            Title = listing.Title,
            Description = listing.Description,
            AddressText = listing.AddressText,
            LocationId = listing.LocationId,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            ListingType = listing.ListingType?.Name,
            BuildingType = listing.BuildingType?.Name,
            PriceAmount = listing.PriceAmount,
            PriceAmountByn = listing.PriceAmountByn,
            PriceAmountUsd = listing.PriceAmountUsd,
            PriceCurrency = listing.PriceCurrency,
            AreaTotal = listing.AreaTotal,
            MainMediaUrl = listing.Media?.FirstOrDefault(x => x.IsMainMedia)?.Url,
            MediaCount = listing.Media?.Count ?? 0,
            PublishedDate = listing.PublishedDate,
            ListingDetailsType = listing.ListingDetailsType
        }, cancellationToken);

        if (!indexed)
        {
            logger.LogWarning(
                "Listing {ListingId} was unhidden, but indexing back into search failed.",
                request.Id);
        }

        return Result.Ok();
    }
}
