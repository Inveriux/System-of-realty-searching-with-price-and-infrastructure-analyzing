using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.DTOs.Locations;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Yandex;
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
    IYandexOrganizationSearchClient yandexOrganizationSearchClient,
    IInfrastructureScoringService scoringService,
    IOptions<YandexOptions> yandexOptions,
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
            SearchRadiusMeters = request.Request.SearchRadiusMeters > 0 ? request.Request.SearchRadiusMeters : yandexOptions.Value.DefaultSearchRadiusMeters,
            Source = "yandex",
            RefreshedAtUtc = DateTime.UtcNow
        };

        foreach (var category in categories)
        {
            var organizations = await yandexOrganizationSearchClient.SearchAsync(
                location.Latitude,
                location.Longitude,
                category.SearchText,
                snapshot.SearchRadiusMeters,
                cancellationToken);

            foreach (var organization in organizations)
            {
                var poi = await poiRepository.UpsertAsync(new Poi
                {
                    Id = Guid.NewGuid(),
                    ExternalId = organization.ExternalId,
                    Name = organization.Name,
                    AddressText = organization.AddressText,
                    Latitude = organization.Latitude,
                    Longitude = organization.Longitude,
                    CategoryId = category.Id
                }, cancellationToken);

                snapshot.Items.Add(new LocationPoiSnapshotItem
                {
                    Id = Guid.NewGuid(),
                    PoiId = poi.Id,
                    Poi = poi,
                    DistanceMeters = organization.DistanceMeters
                });
            }
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
            new PoiCategory { Id = Guid.NewGuid(), Name = "education", SearchText = "school kindergarten university", Weight = 1.2m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "healthcare", SearchText = "hospital clinic pharmacy", Weight = 1.1m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "transport", SearchText = "metro bus stop train", Weight = 1.0m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "commerce", SearchText = "supermarket mall grocery", Weight = 0.9m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = "leisure", SearchText = "park gym cafe cinema", Weight = 0.8m, CreatedAtUtc = DateTime.UtcNow }
        };

        foreach (var category in categories)
            await poiRepository.AddCategoryAsync(category, cancellationToken);

        await poiRepository.SaveChangesAsync(cancellationToken);
        return categories;
    }
}
