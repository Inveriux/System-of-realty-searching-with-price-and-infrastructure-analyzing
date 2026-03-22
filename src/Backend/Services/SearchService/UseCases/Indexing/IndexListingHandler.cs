using BasicBuildingBlocks.BusinessResults;
using SearchService.Data.Repositories.Abstractions;
using SearchService.DTOs.Internal;
using SearchService.Models;
using SearchService.Services.InternalClients.Abstractions;
using MediatR;

namespace SearchService.UseCases.Indexing;

public sealed record IndexListingCommand(IndexListingRequestDto Request) : IRequest<Result>;

public sealed class IndexListingHandler(
    ISearchListingRepository repository,
    IInfrastructureServiceClient infrastructureServiceClient) : IRequestHandler<IndexListingCommand, Result>
{
    public async Task<Result> Handle(IndexListingCommand request, CancellationToken cancellationToken)
    {
        var infrastructureScore = request.Request.LocationId.HasValue
            ? await infrastructureServiceClient.GetInfrastructureScoreAsync(request.Request.LocationId.Value, cancellationToken)
            : null;

        var listing = new SearchListing
        {
            Id = Guid.NewGuid(),
            ListingId = request.Request.ListingId,
            AgentId = request.Request.AgentId,
            Title = request.Request.Title,
            Description = request.Request.Description,
            AddressText = request.Request.AddressText,
            LocationId = request.Request.LocationId,
            Latitude = request.Request.Latitude,
            Longitude = request.Request.Longitude,
            ListingType = request.Request.ListingType,
            BuildingType = request.Request.BuildingType,
            ListingDetailsType = request.Request.ListingDetailsType,
            PriceAmount = request.Request.PriceAmount,
            PriceAmountByn = request.Request.PriceAmountByn,
            PriceAmountUsd = request.Request.PriceAmountUsd,
            PriceCurrency = request.Request.PriceCurrency,
            AreaTotal = request.Request.AreaTotal,
            MainMediaUrl = request.Request.MainMediaUrl,
            PublishedDate = request.Request.PublishedDate,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        SearchListingInfrastructureMapper.Apply(listing, infrastructureScore);

        await repository.UpsertAsync(listing, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
