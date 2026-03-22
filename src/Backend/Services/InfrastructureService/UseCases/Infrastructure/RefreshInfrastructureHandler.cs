using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.DTOs.Locations;
using InfrastructureService.Models;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Summaries.Abstractions;
using InfrastructureService.Services.Yandex.Abstractions;
using InfrastructureService.UseCases.Locations;
using MediatR;
using Microsoft.Extensions.Options;

namespace InfrastructureService.UseCases.Infrastructure;

public sealed record RefreshInfrastructureCommand(RefreshInfrastructureRequestDto Request) : IRequest<Result<InfrastructureSummaryDto>>;

public sealed class RefreshInfrastructureHandler(
    ISender sender,
    ILocationRepository locationRepository,
    IPoiRepository poiRepository,
    IInfrastructureRepository infrastructureRepository,
    IPlacesProvider placesProvider,
    IPlaceCategoryNormalizer placeCategoryNormalizer,
    IInfrastructureScoringService scoringService,
    IInfrastructureSummaryBuilder summaryBuilder,
    IOptions<InfrastructureScoringOptions> scoringOptions,
    IMapper mapper) : IRequestHandler<RefreshInfrastructureCommand, Result<InfrastructureSummaryDto>>
{
    public async Task<Result<InfrastructureSummaryDto>> Handle(RefreshInfrastructureCommand request, CancellationToken cancellationToken)
    {
        var location = await ResolveLocationAsync(request.Request, cancellationToken);
        if (location is null)
            return Result.NotFound<InfrastructureSummaryDto>("Location was not found.");

        var categories = await EnsureDefaultCategoriesAsync(cancellationToken);
        var snapshot = new LocationPoiSnapshot
        {
            Id = Guid.NewGuid(),
            LocationId = location.Id,
            SearchRadiusMeters = request.Request.SearchRadiusMeters > 0 ? request.Request.SearchRadiusMeters : scoringOptions.Value.DefaultRadiusMeters,
            Source = "places-provider",
            RefreshedAtUtc = DateTime.UtcNow
        };

        var nearbyPois = await placesProvider.GetNearbyPoisAsync(
            latitude: (double)location.Latitude,
            longitude: (double)location.Longitude,
            radiusMeters: snapshot.SearchRadiusMeters,
            categories:
            [
                InfrastructureCategory.Education,
                InfrastructureCategory.Healthcare,
                InfrastructureCategory.Transport,
                InfrastructureCategory.Commerce,
                InfrastructureCategory.Leisure
            ],
            limit: 20,
            cancellationToken: cancellationToken);

        foreach (var externalPoi in nearbyPois)
        {
            var normalizationTags = new Dictionary<string, string>(externalPoi.Tags, StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(externalPoi.RawCategory))
                normalizationTags["raw"] = externalPoi.RawCategory!;
            if (externalPoi.RawCategories.Count > 0)
                normalizationTags["raw_categories"] = string.Join('|', externalPoi.RawCategories);

            var normalizedCategory = placeCategoryNormalizer.Normalize(normalizationTags);
            if (string.IsNullOrWhiteSpace(normalizedCategory))
                continue;

            var category = categories.FirstOrDefault(x => x.Name.Equals(normalizedCategory, StringComparison.OrdinalIgnoreCase));
            if (category is null)
                continue;

            var poi = await poiRepository.UpsertAsync(new Poi
            {
                Id = Guid.NewGuid(),
                ExternalId = externalPoi.ExternalId,
                Name = externalPoi.Name,
                AddressText = externalPoi.AddressText,
                Latitude = externalPoi.Latitude,
                Longitude = externalPoi.Longitude,
                CategoryId = category.Id
            }, cancellationToken);

            snapshot.Items.Add(new LocationPoiSnapshotItem
            {
                Id = Guid.NewGuid(),
                PoiId = poi.Id,
                Poi = poi,
                DistanceMeters = externalPoi.DistanceMeters
            });
        }

        await infrastructureRepository.AddSnapshotAsync(snapshot, cancellationToken);
        var score = scoringService.Calculate(location, snapshot);
        await infrastructureRepository.UpsertScoreAsync(score, cancellationToken);
        await infrastructureRepository.SaveChangesAsync(cancellationToken);

        var pois = mapper.Map<IReadOnlyCollection<PoiDto>>(snapshot.Items.OrderBy(x => x.DistanceMeters).ToList());
        var scoreDto = mapper.Map<InfrastructureScoreDto>(score);

        return Result.Ok(new InfrastructureSummaryDto
        {
            LocationId = location.Id,
            AddressText = location.AddressText,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Score = scoreDto,
            Summary = summaryBuilder.BuildSummary(score, snapshot.Items.ToList()),
            Highlights = summaryBuilder.BuildHighlights(score, snapshot.Items.ToList()),
            Pois = pois
        });
    }

    private async Task<Location?> ResolveLocationAsync(RefreshInfrastructureRequestDto request, CancellationToken cancellationToken)
    {
        if (request.LocationId.HasValue)
            return await locationRepository.GetByIdAsync(request.LocationId.Value, cancellationToken);

        var result = await sender.Send(new ResolveLocationCommand(new ResolveLocationRequestDto
        {
            AddressText = request.AddressText,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        }), cancellationToken);

        if (!result.IsSuccess || result.Data is null)
            return null;

        return await locationRepository.GetByIdAsync(result.Data.Id, cancellationToken);
    }

    private async Task<IReadOnlyCollection<PoiCategory>> EnsureDefaultCategoriesAsync(CancellationToken cancellationToken)
    {
        var existing = await poiRepository.GetCategoriesAsync(cancellationToken);
        if (existing.Count > 0)
            return existing;

        var categories = new[]
        {
            new PoiCategory { Id = Guid.NewGuid(), Name = "education", SearchText = "education", Weight = 1.1m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "healthcare", SearchText = "healthcare", Weight = 1.05m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "transport", SearchText = "transport", Weight = 1.2m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "commerce", SearchText = "commerce", Weight = 0.95m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "leisure", SearchText = "leisure", Weight = 0.85m, CreatedAtUtc = DateTime.UtcNow }
        };

        foreach (var category in categories)
            await poiRepository.AddCategoryAsync(category, cancellationToken);

        await poiRepository.SaveChangesAsync(cancellationToken);
        return categories;
    }
}
