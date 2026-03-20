using BasicBuildingBlocks.BusinessResults;
using SearchService.Data.Repositories.Abstractions;
using SearchService.DTOs.Internal;
using SearchService.Models;
using SearchService.Services.InternalClients.Abstractions;
using MediatR;
using System.Text.Json;

namespace SearchService.UseCases.Indexing;

public sealed record IndexListingCommand(IndexListingRequestDto Request) : IRequest<Result>;

public sealed class IndexListingHandler(
    ISearchListingRepository repository,
    IInfrastructureServiceClient infrastructureServiceClient) : IRequestHandler<IndexListingCommand, Result>
{
    public async Task<Result> Handle(IndexListingCommand request, CancellationToken cancellationToken)
    {
        var features = request.Request.LocationId.HasValue
            ? await infrastructureServiceClient.GetSearchFeaturesAsync(request.Request.LocationId.Value, cancellationToken)
            : [];

        var listing = new SearchListing
        {
            Id = Guid.NewGuid(),
            ListingId = request.Request.ListingId,
            Title = request.Request.Title,
            Description = request.Request.Description,
            AddressText = request.Request.AddressText,
            LocationId = request.Request.LocationId,
            Latitude = request.Request.Latitude,
            Longitude = request.Request.Longitude,
            ListingType = request.Request.ListingType,
            BuildingType = request.Request.BuildingType,
            PriceAmount = request.Request.PriceAmount,
            PriceCurrency = request.Request.PriceCurrency,
            AreaTotal = request.Request.AreaTotal,
            MainMediaUrl = request.Request.MainMediaUrl,
            PublishedDate = request.Request.PublishedDate,
            InfrastructureScore = features.Count == 0 ? null : features.Values.Sum(),
            SearchFeaturesJson = JsonSerializer.Serialize(features),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await repository.UpsertAsync(listing, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
