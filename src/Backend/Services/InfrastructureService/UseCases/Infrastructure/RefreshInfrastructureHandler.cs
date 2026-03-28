using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.DTOs.Locations;
using InfrastructureService.Models;
using InfrastructureService.Services.Classification.Abstractions;
using InfrastructureService.Services.Configuration;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Routing.Abstractions;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Sanitization.Abstractions;
using InfrastructureService.Services.Summaries.Abstractions;
using InfrastructureService.Services.TransportGrouping;
using InfrastructureService.Services.TransportGrouping.Abstractions;
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
    IPoiClassificationService poiClassificationService,
    IPoiSanitizationService poiSanitizationService,
    IInfrastructureScoringService scoringService,
    IInfrastructureSummaryBuilder summaryBuilder,
    IRouteMetricsService routeMetricsService,
    ITransportStopGroupingService transportStopGroupingService,
    IOptions<InfrastructureScoringOptions> scoringOptions,
    IMapper mapper) : IRequestHandler<RefreshInfrastructureCommand, Result<InfrastructureSummaryDto>>
{
    private static readonly HashSet<string> KnownMetroStationNames =
    [
        "малиновка",
        "петровщина",
        "михалово",
        "грушевка",
        "институт культуры",
        "площадь ленина",
        "октябрьская",
        "площадь победы",
        "площадь якуба коласа",
        "академия наук",
        "парк челюскинцев",
        "московская",
        "восток",
        "борисовский тракт",
        "уручье",
        "могилевская",
        "автозаводская",
        "партизанская",
        "тракторный завод",
        "пролетарская",
        "первомайская",
        "купаловская",
        "немига",
        "фрунзенская",
        "юбилейная площадь",
        "молодежная",
        "пушкинская",
        "спортивная",
        "кунцевщина",
        "каменная горка",
        "ковальская слобода",
        "вокзальная",
        "площадь франтишка богушевича",
        "аэродромная"
    ];

    public async Task<Result<InfrastructureSummaryDto>> Handle(RefreshInfrastructureCommand request, CancellationToken cancellationToken)
    {
        var location = await ResolveLocationAsync(request.Request, cancellationToken);
        if (location is null)
            return Result.NotFound<InfrastructureSummaryDto>("Location was not found.");

        var categories = await EnsureDefaultCategoriesAsync(cancellationToken);
        var requestedRadius = request.Request.SearchRadiusMeters > 0
            ? request.Request.SearchRadiusMeters
            : scoringOptions.Value.DefaultRadiusMeters;
        var supersetRadius = Math.Max(requestedRadius, InfrastructureMetadata.GetSupersetRadiusMeters());
        var snapshot = new LocationPoiSnapshot
        {
            Id = Guid.NewGuid(),
            LocationId = location.Id,
            SearchRadiusMeters = supersetRadius,
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
            limit: scoringOptions.Value.MaxPoiCount,
            cancellationToken: cancellationToken);

        var excludedPoiReasons = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var externalPoi in nearbyPois)
        {
            var sanitization = poiSanitizationService.Evaluate(externalPoi);
            if (sanitization.ShouldExclude)
            {
                if (!string.IsNullOrWhiteSpace(sanitization.Reason))
                {
                    excludedPoiReasons[sanitization.Reason] = excludedPoiReasons.GetValueOrDefault(sanitization.Reason) + 1;
                }

                continue;
            }

            var classification = poiClassificationService.Classify(externalPoi);
            if (classification is null || string.IsNullOrWhiteSpace(classification.CategoryCode))
                continue;

            if (!ShouldIncludePoi(classification.CategoryCode, classification.SubcategoryCode, externalPoi.DistanceMeters))
                continue;

            var category = categories.FirstOrDefault(x => x.Name.Equals(classification.CategoryCode, StringComparison.OrdinalIgnoreCase));
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
                CategoryId = category.Id,
                SubcategoryCode = classification.SubcategoryCode,
                Source = externalPoi.Source
            }, cancellationToken);

            snapshot.Items.Add(new LocationPoiSnapshotItem
            {
                Id = Guid.NewGuid(),
                PoiId = poi.Id,
                Poi = poi,
                DistanceMeters = externalPoi.DistanceMeters
            });
        }

        var sortedItems = snapshot.Items.OrderBy(x => x.DistanceMeters).ToArray();
        var detailsBase = await BuildDetailsAsync(location, sortedItems, excludedPoiReasons, cancellationToken);
        var preliminaryScore = scoringService.Calculate(location, snapshot, detailsBase);
        var highlights = summaryBuilder.BuildHighlights(preliminaryScore, detailsBase, sortedItems);
        var summary = summaryBuilder.BuildSummary(preliminaryScore, detailsBase, sortedItems);
        var details = new InfrastructureDetailsDto
        {
            NearbyCategories = detailsBase.NearbyCategories,
            TransportDetails = detailsBase.TransportDetails,
            SearchFeatures = detailsBase.SearchFeatures,
            FilteredAddressLikePoiCount = detailsBase.FilteredAddressLikePoiCount,
            ExcludedPoiReasons = detailsBase.ExcludedPoiReasons,
            PrimaryRadiusConfig = detailsBase.PrimaryRadiusConfig,
            FallbackRadiusConfig = detailsBase.FallbackRadiusConfig,
            MissingPrimaryCategories = detailsBase.MissingPrimaryCategories,
            ClassificationVersions = detailsBase.ClassificationVersions,
            Highlights = highlights
        };

        await infrastructureRepository.AddSnapshotAsync(snapshot, cancellationToken);
        var score = scoringService.Calculate(location, snapshot, details);
        await infrastructureRepository.UpsertScoreAsync(score, cancellationToken);
        await infrastructureRepository.SaveChangesAsync(cancellationToken);

        var pois = mapper.Map<IReadOnlyCollection<PoiDto>>(sortedItems);
        var scoreDto = mapper.Map<InfrastructureScoreDto>(score);

        return Result.Ok(new InfrastructureSummaryDto
        {
            LocationId = location.Id,
            AddressText = location.AddressText,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Score = scoreDto,
            Summary = summary,
            Highlights = highlights,
            Pois = pois,
            NearbyCategories = details.NearbyCategories,
            TransportDetails = details.TransportDetails,
            SearchFeatures = details.SearchFeatures,
            PrimaryRadiusConfig = details.PrimaryRadiusConfig,
            FallbackRadiusConfig = details.FallbackRadiusConfig,
            MissingPrimaryCategories = details.MissingPrimaryCategories,
            ClassificationVersions = details.ClassificationVersions
        });
    }

    private static bool ShouldIncludePoi(string categoryCode, string? subcategoryCode, decimal distanceMeters)
    {
        if (!categoryCode.Equals(InfrastructureCategoryCodes.Healthcare, StringComparison.OrdinalIgnoreCase))
        {
            if (categoryCode.Equals(InfrastructureCategoryCodes.Transport, StringComparison.OrdinalIgnoreCase)
                && string.Equals(subcategoryCode, "parking", StringComparison.OrdinalIgnoreCase))
            {
                return distanceMeters <= 500m;
            }

            return true;
        }

        if (string.Equals(subcategoryCode, "hospital", StringComparison.OrdinalIgnoreCase))
            return distanceMeters <= 5000m;

        return distanceMeters <= 1500m;
    }

    private async Task<InfrastructureDetailsDto> BuildDetailsAsync(
        Location location,
        IReadOnlyCollection<LocationPoiSnapshotItem> items,
        IReadOnlyDictionary<string, int> excludedPoiReasons,
        CancellationToken cancellationToken)
    {
        var stopGroups = transportStopGroupingService.Group(items);
        var subcategoryDtos = new List<InfrastructureNearbyCategoryDto>();
        foreach (var definition in InfrastructureMetadata.SupportedSubcategories)
        {
            subcategoryDtos.Add(await BuildNearbyCategoryAsync(
                location,
                items,
                definition.CategoryCode,
                definition.SubcategoryCode,
                [definition.SubcategoryCode],
                definition.PrimaryRadiusMeters,
                definition.FallbackRadiusMeters,
                cancellationToken));
        }

        var featureDtos = new Dictionary<string, InfrastructureNearbyCategoryDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in InfrastructureMetadata.SearchFeatures)
        {
            if (definition.SubcategoryCode.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                featureDtos[definition.SubcategoryCode] = await BuildStopGroupCategoryAsync(
                    location,
                    stopGroups,
                    definition.CategoryCode,
                    definition.SubcategoryCode,
                    definition.PrimaryRadiusMeters,
                    definition.FallbackRadiusMeters,
                    cancellationToken);
                continue;
            }

            featureDtos[definition.SubcategoryCode] = await BuildNearbyCategoryAsync(
                location,
                items,
                definition.CategoryCode,
                definition.SubcategoryCode,
                definition.SourceSubcategories,
                definition.PrimaryRadiusMeters,
                definition.FallbackRadiusMeters,
                cancellationToken);
        }

        var searchFeatures = BuildSearchFeatures(featureDtos, stopGroups, items);
        var transportDetails = new InfrastructureTransportDetailsDto
        {
            NearestStopDistanceM = searchFeatures.NearestStopDistanceM,
            NearestStopWalkMinutes = searchFeatures.NearestStopWalkMinutes,
            StopsCountPrimary = searchFeatures.StopsCountPrimary,
            StopGroupsCountPrimary = searchFeatures.StopGroupsCountPrimary,
            StopPlatformsCountPrimary = searchFeatures.StopPlatformsCountPrimary,
            NearestStopGroupDistanceM = searchFeatures.NearestStopGroupDistanceM,
            NearestStopGroupWalkMinutes = searchFeatures.NearestStopGroupWalkMinutes,
            NearestStopGroupDriveMinutes = searchFeatures.NearestStopGroupDriveMinutes,
            NearestStopGroupDriveMinutesEstimated = searchFeatures.NearestStopGroupDriveMinutesEstimated,
            NearestStopGroupName = searchFeatures.NearestStopGroupName,
            NearestStopGroupPlatformCount = searchFeatures.NearestStopGroupPlatformCount,
            NearestStopGroupAnyDistanceM = searchFeatures.NearestStopGroupAnyDistanceM,
            NearestStopGroupAnyWalkMinutes = searchFeatures.NearestStopGroupAnyWalkMinutes,
            NearestStopGroupAnyDriveMinutes = searchFeatures.NearestStopGroupAnyDriveMinutes,
            NearestStopGroupAnyDriveMinutesEstimated = searchFeatures.NearestStopGroupAnyDriveMinutesEstimated,
            NearestStopGroupFoundRadiusM = searchFeatures.NearestStopGroupFoundRadiusM,
            StopGroups = stopGroups.Select(static group => new InfrastructureTransportStopGroupDto
            {
                GroupId = group.GroupId,
                NormalizedName = group.NormalizedName,
                DisplayName = group.DisplayName,
                CenterLatitude = group.CenterLatitude,
                CenterLongitude = group.CenterLongitude,
                MaxDistanceWithinGroup = group.MaxDistanceWithinGroup,
                PointCount = group.PointCount,
                PlatformCount = group.PlatformCount,
                StopTypes = group.StopTypes,
                SourcePoiIds = group.SourcePoiIds,
                NearestDistanceMeters = group.NearestDistanceMeters
            }).ToArray(),
            NearestMetroDistanceM = searchFeatures.NearestMetroDistanceM,
            NearestMetroWalkMinutes = searchFeatures.NearestMetroWalkMinutes,
            NearestMetroName = searchFeatures.NearestMetroName,
            MetroFoundInPrimaryRadius = searchFeatures.MetroFoundInPrimaryRadius,
            NearestParkingDistanceM = searchFeatures.NearestParkingDistanceM,
            NearestParkingWalkMinutes = searchFeatures.NearestParkingWalkMinutes,
            NearestParkingDriveMinutes = searchFeatures.NearestParkingDriveMinutes,
            NearestParkingDriveMinutesEstimated = searchFeatures.NearestParkingDriveMinutesEstimated,
            ParkingCountPrimary = searchFeatures.ParkingCountPrimary
        };

        return new InfrastructureDetailsDto
        {
            NearbyCategories = subcategoryDtos,
            TransportDetails = transportDetails,
            SearchFeatures = searchFeatures,
            FilteredAddressLikePoiCount = excludedPoiReasons.Values.Sum(),
            ExcludedPoiReasons = new Dictionary<string, int>(excludedPoiReasons, StringComparer.OrdinalIgnoreCase),
            PrimaryRadiusConfig = InfrastructureMetadata.SupportedSubcategories.ToDictionary(
                static x => $"{x.CategoryCode}.{x.SubcategoryCode}",
                static x => x.PrimaryRadiusMeters,
                StringComparer.OrdinalIgnoreCase),
            FallbackRadiusConfig = InfrastructureMetadata.SupportedSubcategories.ToDictionary(
                static x => $"{x.CategoryCode}.{x.SubcategoryCode}",
                static x => x.FallbackRadiusMeters,
                StringComparer.OrdinalIgnoreCase),
            MissingPrimaryCategories = featureDtos.Values
                .Where(static x => !x.FoundInPrimaryRadius)
                .Select(static x => x.SubcategoryCode)
                .OrderBy(static x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            ClassificationVersions = poiClassificationService.GetDictionaryVersions()
        };
    }

    private async Task<InfrastructureNearbyCategoryDto> BuildStopGroupCategoryAsync(
        Location location,
        IReadOnlyCollection<TransportStopGroup> stopGroups,
        string categoryCode,
        string subcategoryCode,
        int primaryRadiusMeters,
        int? fallbackRadiusMeters,
        CancellationToken cancellationToken)
    {
        var primaryGroups = stopGroups
            .Where(group => group.NearestDistanceMeters <= primaryRadiusMeters)
            .OrderBy(static group => group.NearestDistanceMeters)
            .ToArray();
        var nearestPrimary = primaryGroups.FirstOrDefault();
        var nearestAny = nearestPrimary;

        if (nearestAny is null && fallbackRadiusMeters.HasValue)
        {
            nearestAny = stopGroups
                .Where(group => group.NearestDistanceMeters <= fallbackRadiusMeters.Value)
                .OrderBy(static group => group.NearestDistanceMeters)
                .FirstOrDefault();
        }

        var nearestPrimaryMetrics = nearestPrimary is null
            ? null
            : await routeMetricsService.GetMetricsAsync(
                location.Latitude,
                location.Longitude,
                nearestPrimary.RepresentativeLatitude,
                nearestPrimary.RepresentativeLongitude,
                cancellationToken);

        var nearestAnyMetrics = nearestAny is null
            ? null
            : nearestPrimary is not null && nearestAny.GroupId == nearestPrimary.GroupId
                ? nearestPrimaryMetrics
                : await routeMetricsService.GetMetricsAsync(
                    location.Latitude,
                    location.Longitude,
                    nearestAny.RepresentativeLatitude,
                    nearestAny.RepresentativeLongitude,
                    cancellationToken);

        return new InfrastructureNearbyCategoryDto
        {
            CategoryCode = categoryCode,
            SubcategoryCode = subcategoryCode,
            CountInPrimaryRadius = primaryGroups.Length,
            NearestDistanceM = nearestPrimaryMetrics?.DistanceMeters,
            NearestWalkMinutes = nearestPrimaryMetrics?.WalkMinutes,
            NearestDriveMinutes = nearestPrimaryMetrics?.DriveMinutes,
            NearestDriveMinutesEstimated = nearestPrimaryMetrics?.IsDriveMinutesEstimated ?? false,
            NearestAnyDistanceM = nearestAnyMetrics?.DistanceMeters,
            NearestAnyWalkMinutes = nearestAnyMetrics?.WalkMinutes,
            NearestAnyDriveMinutes = nearestAnyMetrics?.DriveMinutes,
            NearestAnyDriveMinutesEstimated = nearestAnyMetrics?.IsDriveMinutesEstimated ?? false,
            FoundInRadiusM = nearestPrimary is not null
                ? primaryRadiusMeters
                : nearestAny is not null && fallbackRadiusMeters.HasValue
                    ? fallbackRadiusMeters.Value
                    : null,
            FoundInPrimaryRadius = nearestPrimary is not null
        };
    }

    private async Task<InfrastructureNearbyCategoryDto> BuildNearbyCategoryAsync(
        Location location,
        IReadOnlyCollection<LocationPoiSnapshotItem> items,
        string categoryCode,
        string subcategoryCode,
        IReadOnlyCollection<string> sourceSubcategories,
        int primaryRadiusMeters,
        int? fallbackRadiusMeters,
        CancellationToken cancellationToken)
    {
        var matchedItems = items
            .Where(x => x.Poi?.Category?.Name.Equals(categoryCode, StringComparison.OrdinalIgnoreCase) == true)
            .Where(x => MatchesFeatureSubcategory(x, sourceSubcategories))
            .OrderBy(x => x.DistanceMeters)
            .ToArray();
        matchedItems = DeduplicateFeatureItems(matchedItems, categoryCode);

        var primaryItems = matchedItems
            .Where(x => x.DistanceMeters <= primaryRadiusMeters)
            .ToArray();
        var nearestPrimary = primaryItems.FirstOrDefault();
        var nearestAny = nearestPrimary;

        if (nearestAny is null && fallbackRadiusMeters.HasValue)
        {
            nearestAny = matchedItems
                .FirstOrDefault(x => x.DistanceMeters <= fallbackRadiusMeters.Value);
        }

        var nearestPrimaryMetrics = nearestPrimary is null
            ? null
            : await routeMetricsService.GetMetricsAsync(
                location.Latitude,
                location.Longitude,
                nearestPrimary.Poi!.Latitude,
                nearestPrimary.Poi.Longitude,
                cancellationToken);

        var nearestAnyMetrics = nearestAny is null
            ? null
            : nearestPrimary is not null && nearestAny.Id == nearestPrimary.Id
                ? nearestPrimaryMetrics
                : await routeMetricsService.GetMetricsAsync(
                    location.Latitude,
                    location.Longitude,
                    nearestAny.Poi!.Latitude,
                    nearestAny.Poi.Longitude,
                    cancellationToken);

        return new InfrastructureNearbyCategoryDto
        {
            CategoryCode = categoryCode,
            SubcategoryCode = subcategoryCode,
            CountInPrimaryRadius = primaryItems.Length,
            NearestDistanceM = nearestPrimaryMetrics?.DistanceMeters,
            NearestWalkMinutes = nearestPrimaryMetrics?.WalkMinutes,
            NearestDriveMinutes = nearestPrimaryMetrics?.DriveMinutes,
            NearestDriveMinutesEstimated = nearestPrimaryMetrics?.IsDriveMinutesEstimated ?? false,
            NearestAnyDistanceM = nearestAnyMetrics?.DistanceMeters,
            NearestAnyWalkMinutes = nearestAnyMetrics?.WalkMinutes,
            NearestAnyDriveMinutes = nearestAnyMetrics?.DriveMinutes,
            NearestAnyDriveMinutesEstimated = nearestAnyMetrics?.IsDriveMinutesEstimated ?? false,
            FoundInRadiusM = nearestPrimary is not null
                ? primaryRadiusMeters
                : nearestAny is not null && fallbackRadiusMeters.HasValue
                    ? fallbackRadiusMeters.Value
                    : null,
            FoundInPrimaryRadius = nearestPrimary is not null
        };
    }

    private static bool MatchesFeatureSubcategory(
        LocationPoiSnapshotItem item,
        IReadOnlyCollection<string> sourceSubcategories)
    {
        var poi = item.Poi;
        if (poi is null)
            return false;

        if (!string.IsNullOrWhiteSpace(poi.SubcategoryCode)
            && sourceSubcategories.Contains(poi.SubcategoryCode, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        if (sourceSubcategories.Contains("metro_station", StringComparer.OrdinalIgnoreCase))
        {
            var name = poi.Name?.Trim().ToLowerInvariant() ?? string.Empty;
            var address = poi.AddressText?.Trim().ToLowerInvariant() ?? string.Empty;

            if (name.Contains("метро", StringComparison.OrdinalIgnoreCase)
                || name.Contains("metro", StringComparison.OrdinalIgnoreCase)
                || name.Contains("станция метро", StringComparison.OrdinalIgnoreCase)
                || KnownMetroStationNames.Contains(name)
                || address.Contains("метро", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static LocationPoiSnapshotItem[] DeduplicateFeatureItems(
        IReadOnlyCollection<LocationPoiSnapshotItem> items,
        string categoryCode)
    {
        if (!categoryCode.Equals(InfrastructureCategoryCodes.Leisure, StringComparison.OrdinalIgnoreCase))
            return items.ToArray();

        return items
            .GroupBy(BuildLeisureDeduplicationKey, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.OrderBy(x => x.DistanceMeters).First())
            .OrderBy(static x => x.DistanceMeters)
            .ToArray();
    }

    private static string BuildLeisureDeduplicationKey(LocationPoiSnapshotItem item)
    {
        var poi = item.Poi;
        var name = poi?.Name?.Trim().ToLowerInvariant() ?? string.Empty;
        var subcategory = poi?.SubcategoryCode?.Trim().ToLowerInvariant() ?? string.Empty;
        var latitude = poi is null ? 0m : decimal.Round(poi.Latitude, 4);
        var longitude = poi is null ? 0m : decimal.Round(poi.Longitude, 4);
        return $"{subcategory}:{name}:{latitude}:{longitude}";
    }

    private static InfrastructureSearchFeaturesDto BuildSearchFeatures(
        IReadOnlyDictionary<string, InfrastructureNearbyCategoryDto> featureDtos,
        IReadOnlyCollection<TransportStopGroup> stopGroups,
        IReadOnlyCollection<LocationPoiSnapshotItem> items)
    {
        var store = featureDtos["store"];
        var pharmacy = featureDtos["pharmacy"];
        var stop = featureDtos["stop"];
        var parking = featureDtos["parking"];
        var school = featureDtos["school"];
        var kindergarten = featureDtos["kindergarten"];
        var clinic = featureDtos["clinic"];
        var hospital = featureDtos["hospital"];
        var leisure = featureDtos["leisure"];
        var park = featureDtos["park"];
        var fitness = featureDtos["fitness"];
        var museum = featureDtos["museum"];
        var theater = featureDtos["theater"];
        var cinema = featureDtos["cinema"];
        var cafe = featureDtos["cafe"];
        var restaurant = featureDtos["restaurant"];
        var metro = featureDtos["metro_station"];
        var nearestMetroName = items
            .Where(static x => string.Equals(x.Poi?.SubcategoryCode, "metro_station", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static x => x.DistanceMeters)
            .Select(static x => x.Poi!.Name)
            .FirstOrDefault();
        var primaryStopGroups = stopGroups
            .Where(group => stop.FoundInPrimaryRadius && group.NearestDistanceMeters <= (stop.FoundInRadiusM ?? int.MaxValue))
            .ToArray();
        var nearestStopGroup = stopGroups.OrderBy(static group => group.NearestDistanceMeters).FirstOrDefault();

        return new InfrastructureSearchFeaturesDto
        {
            StoreCountPrimary = store.CountInPrimaryRadius,
            NearestStoreDistanceM = store.NearestDistanceM,
            NearestStoreWalkMinutes = store.NearestWalkMinutes,
            NearestStoreDriveMinutes = store.NearestDriveMinutes,
            NearestStoreDriveMinutesEstimated = store.NearestDriveMinutesEstimated,
            NearestStoreAnyDistanceM = store.NearestAnyDistanceM,
            NearestStoreAnyWalkMinutes = store.NearestAnyWalkMinutes,
            NearestStoreAnyDriveMinutes = store.NearestAnyDriveMinutes,
            NearestStoreAnyDriveMinutesEstimated = store.NearestAnyDriveMinutesEstimated,
            NearestStoreFoundRadiusM = store.FoundInRadiusM,

            PharmacyCountPrimary = pharmacy.CountInPrimaryRadius,
            NearestPharmacyDistanceM = pharmacy.NearestDistanceM,
            NearestPharmacyWalkMinutes = pharmacy.NearestWalkMinutes,
            NearestPharmacyDriveMinutes = pharmacy.NearestDriveMinutes,
            NearestPharmacyDriveMinutesEstimated = pharmacy.NearestDriveMinutesEstimated,
            NearestPharmacyAnyDistanceM = pharmacy.NearestAnyDistanceM,
            NearestPharmacyAnyWalkMinutes = pharmacy.NearestAnyWalkMinutes,
            NearestPharmacyAnyDriveMinutes = pharmacy.NearestAnyDriveMinutes,
            NearestPharmacyAnyDriveMinutesEstimated = pharmacy.NearestAnyDriveMinutesEstimated,
            NearestPharmacyFoundRadiusM = pharmacy.FoundInRadiusM,

            StopsCountPrimary = stop.CountInPrimaryRadius,
            NearestStopDistanceM = stop.NearestDistanceM,
            NearestStopWalkMinutes = stop.NearestWalkMinutes,
            NearestStopDriveMinutes = stop.NearestDriveMinutes,
            NearestStopDriveMinutesEstimated = stop.NearestDriveMinutesEstimated,
            NearestStopAnyDistanceM = stop.NearestAnyDistanceM,
            NearestStopAnyWalkMinutes = stop.NearestAnyWalkMinutes,
            NearestStopAnyDriveMinutes = stop.NearestAnyDriveMinutes,
            NearestStopAnyDriveMinutesEstimated = stop.NearestAnyDriveMinutesEstimated,
            NearestStopFoundRadiusM = stop.FoundInRadiusM,
            StopGroupsCountPrimary = stop.CountInPrimaryRadius,
            StopPlatformsCountPrimary = primaryStopGroups.Sum(static group => group.PlatformCount),
            NearestStopGroupDistanceM = stop.NearestDistanceM,
            NearestStopGroupWalkMinutes = stop.NearestWalkMinutes,
            NearestStopGroupDriveMinutes = stop.NearestDriveMinutes,
            NearestStopGroupDriveMinutesEstimated = stop.NearestDriveMinutesEstimated,
            NearestStopGroupName = nearestStopGroup?.DisplayName,
            NearestStopGroupPlatformCount = nearestStopGroup?.PlatformCount,
            NearestStopGroupAnyDistanceM = stop.NearestAnyDistanceM,
            NearestStopGroupAnyWalkMinutes = stop.NearestAnyWalkMinutes,
            NearestStopGroupAnyDriveMinutes = stop.NearestAnyDriveMinutes,
            NearestStopGroupAnyDriveMinutesEstimated = stop.NearestAnyDriveMinutesEstimated,
            NearestStopGroupFoundRadiusM = stop.FoundInRadiusM,

            ParkingCountPrimary = parking.CountInPrimaryRadius,
            NearestParkingDistanceM = parking.NearestDistanceM,
            NearestParkingWalkMinutes = parking.NearestWalkMinutes,
            NearestParkingDriveMinutes = parking.NearestDriveMinutes,
            NearestParkingDriveMinutesEstimated = parking.NearestDriveMinutesEstimated,
            NearestParkingAnyDistanceM = parking.NearestAnyDistanceM,
            NearestParkingAnyWalkMinutes = parking.NearestAnyWalkMinutes,
            NearestParkingAnyDriveMinutes = parking.NearestAnyDriveMinutes,
            NearestParkingAnyDriveMinutesEstimated = parking.NearestAnyDriveMinutesEstimated,
            NearestParkingFoundRadiusM = parking.FoundInRadiusM,

            SchoolCountPrimary = school.CountInPrimaryRadius,
            NearestSchoolDistanceM = school.NearestDistanceM,
            NearestSchoolWalkMinutes = school.NearestWalkMinutes,
            NearestSchoolDriveMinutes = school.NearestDriveMinutes,
            NearestSchoolDriveMinutesEstimated = school.NearestDriveMinutesEstimated,
            NearestSchoolAnyDistanceM = school.NearestAnyDistanceM,
            NearestSchoolAnyWalkMinutes = school.NearestAnyWalkMinutes,
            NearestSchoolAnyDriveMinutes = school.NearestAnyDriveMinutes,
            NearestSchoolAnyDriveMinutesEstimated = school.NearestAnyDriveMinutesEstimated,
            NearestSchoolFoundRadiusM = school.FoundInRadiusM,

            KindergartenCountPrimary = kindergarten.CountInPrimaryRadius,
            NearestKindergartenDistanceM = kindergarten.NearestDistanceM,
            NearestKindergartenWalkMinutes = kindergarten.NearestWalkMinutes,
            NearestKindergartenDriveMinutes = kindergarten.NearestDriveMinutes,
            NearestKindergartenDriveMinutesEstimated = kindergarten.NearestDriveMinutesEstimated,
            NearestKindergartenAnyDistanceM = kindergarten.NearestAnyDistanceM,
            NearestKindergartenAnyWalkMinutes = kindergarten.NearestAnyWalkMinutes,
            NearestKindergartenAnyDriveMinutes = kindergarten.NearestAnyDriveMinutes,
            NearestKindergartenAnyDriveMinutesEstimated = kindergarten.NearestAnyDriveMinutesEstimated,
            NearestKindergartenFoundRadiusM = kindergarten.FoundInRadiusM,

            ClinicCountPrimary = clinic.CountInPrimaryRadius,
            NearestClinicDistanceM = clinic.NearestDistanceM,
            NearestClinicWalkMinutes = clinic.NearestWalkMinutes,
            NearestClinicDriveMinutes = clinic.NearestDriveMinutes,
            NearestClinicDriveMinutesEstimated = clinic.NearestDriveMinutesEstimated,
            NearestClinicAnyDistanceM = clinic.NearestAnyDistanceM,
            NearestClinicAnyWalkMinutes = clinic.NearestAnyWalkMinutes,
            NearestClinicAnyDriveMinutes = clinic.NearestAnyDriveMinutes,
            NearestClinicAnyDriveMinutesEstimated = clinic.NearestAnyDriveMinutesEstimated,
            NearestClinicFoundRadiusM = clinic.FoundInRadiusM,

            HospitalCountPrimary = hospital.CountInPrimaryRadius,
            NearestHospitalDistanceM = hospital.NearestDistanceM,
            NearestHospitalWalkMinutes = hospital.NearestWalkMinutes,
            NearestHospitalDriveMinutes = hospital.NearestDriveMinutes,
            NearestHospitalDriveMinutesEstimated = hospital.NearestDriveMinutesEstimated,
            NearestHospitalAnyDistanceM = hospital.NearestAnyDistanceM,
            NearestHospitalAnyWalkMinutes = hospital.NearestAnyWalkMinutes,
            NearestHospitalAnyDriveMinutes = hospital.NearestAnyDriveMinutes,
            NearestHospitalAnyDriveMinutesEstimated = hospital.NearestAnyDriveMinutesEstimated,
            NearestHospitalFoundRadiusM = hospital.FoundInRadiusM,

            LeisureCountPrimary = leisure.CountInPrimaryRadius,
            NearestLeisureDistanceM = leisure.NearestDistanceM,
            NearestLeisureWalkMinutes = leisure.NearestWalkMinutes,
            NearestLeisureDriveMinutes = leisure.NearestDriveMinutes,
            NearestLeisureDriveMinutesEstimated = leisure.NearestDriveMinutesEstimated,
            NearestLeisureAnyDistanceM = leisure.NearestAnyDistanceM,
            NearestLeisureAnyWalkMinutes = leisure.NearestAnyWalkMinutes,
            NearestLeisureAnyDriveMinutes = leisure.NearestAnyDriveMinutes,
            NearestLeisureAnyDriveMinutesEstimated = leisure.NearestAnyDriveMinutesEstimated,
            NearestLeisureFoundRadiusM = leisure.FoundInRadiusM,

            ParksCountPrimary = park.CountInPrimaryRadius,
            NearestParkDistanceM = park.NearestDistanceM,
            NearestParkWalkMinutes = park.NearestWalkMinutes,
            NearestParkDriveMinutes = park.NearestDriveMinutes,
            NearestParkDriveMinutesEstimated = park.NearestDriveMinutesEstimated,
            NearestParkAnyDistanceM = park.NearestAnyDistanceM,
            NearestParkAnyWalkMinutes = park.NearestAnyWalkMinutes,
            NearestParkAnyDriveMinutes = park.NearestAnyDriveMinutes,
            NearestParkAnyDriveMinutesEstimated = park.NearestAnyDriveMinutesEstimated,
            NearestParkFoundRadiusM = park.FoundInRadiusM,

            FitnessCountPrimary = fitness.CountInPrimaryRadius,
            NearestFitnessDistanceM = fitness.NearestDistanceM,
            NearestFitnessWalkMinutes = fitness.NearestWalkMinutes,
            NearestFitnessDriveMinutes = fitness.NearestDriveMinutes,
            NearestFitnessDriveMinutesEstimated = fitness.NearestDriveMinutesEstimated,
            NearestFitnessAnyDistanceM = fitness.NearestAnyDistanceM,
            NearestFitnessAnyWalkMinutes = fitness.NearestAnyWalkMinutes,
            NearestFitnessAnyDriveMinutes = fitness.NearestAnyDriveMinutes,
            NearestFitnessAnyDriveMinutesEstimated = fitness.NearestAnyDriveMinutesEstimated,
            NearestFitnessFoundRadiusM = fitness.FoundInRadiusM,

            MuseumsCountPrimary = museum.CountInPrimaryRadius,
            NearestMuseumDistanceM = museum.NearestDistanceM,
            NearestMuseumWalkMinutes = museum.NearestWalkMinutes,
            NearestMuseumDriveMinutes = museum.NearestDriveMinutes,
            NearestMuseumDriveMinutesEstimated = museum.NearestDriveMinutesEstimated,
            NearestMuseumAnyDistanceM = museum.NearestAnyDistanceM,
            NearestMuseumAnyWalkMinutes = museum.NearestAnyWalkMinutes,
            NearestMuseumAnyDriveMinutes = museum.NearestAnyDriveMinutes,
            NearestMuseumAnyDriveMinutesEstimated = museum.NearestAnyDriveMinutesEstimated,
            NearestMuseumFoundRadiusM = museum.FoundInRadiusM,

            TheatersCountPrimary = theater.CountInPrimaryRadius,
            NearestTheaterDistanceM = theater.NearestDistanceM,
            NearestTheaterWalkMinutes = theater.NearestWalkMinutes,
            NearestTheaterDriveMinutes = theater.NearestDriveMinutes,
            NearestTheaterDriveMinutesEstimated = theater.NearestDriveMinutesEstimated,
            NearestTheaterAnyDistanceM = theater.NearestAnyDistanceM,
            NearestTheaterAnyWalkMinutes = theater.NearestAnyWalkMinutes,
            NearestTheaterAnyDriveMinutes = theater.NearestAnyDriveMinutes,
            NearestTheaterAnyDriveMinutesEstimated = theater.NearestAnyDriveMinutesEstimated,
            NearestTheaterFoundRadiusM = theater.FoundInRadiusM,

            CinemasCountPrimary = cinema.CountInPrimaryRadius,
            NearestCinemaDistanceM = cinema.NearestDistanceM,
            NearestCinemaWalkMinutes = cinema.NearestWalkMinutes,
            NearestCinemaDriveMinutes = cinema.NearestDriveMinutes,
            NearestCinemaDriveMinutesEstimated = cinema.NearestDriveMinutesEstimated,
            NearestCinemaAnyDistanceM = cinema.NearestAnyDistanceM,
            NearestCinemaAnyWalkMinutes = cinema.NearestAnyWalkMinutes,
            NearestCinemaAnyDriveMinutes = cinema.NearestAnyDriveMinutes,
            NearestCinemaAnyDriveMinutesEstimated = cinema.NearestAnyDriveMinutesEstimated,
            NearestCinemaFoundRadiusM = cinema.FoundInRadiusM,

            CafesCountPrimary = cafe.CountInPrimaryRadius,
            RestaurantsCountPrimary = restaurant.CountInPrimaryRadius,

            NearestMetroDistanceM = metro.NearestDistanceM,
            NearestMetroWalkMinutes = metro.NearestWalkMinutes,
            NearestMetroName = nearestMetroName,
            MetroFoundInPrimaryRadius = metro.FoundInPrimaryRadius
        };
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
            new PoiCategory { Id = Guid.NewGuid(), Name = InfrastructureCategoryCodes.Education, SearchText = InfrastructureCategoryCodes.Education, Weight = 1.1m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = InfrastructureCategoryCodes.Healthcare, SearchText = InfrastructureCategoryCodes.Healthcare, Weight = 1.05m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = InfrastructureCategoryCodes.Transport, SearchText = InfrastructureCategoryCodes.Transport, Weight = 1.2m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = InfrastructureCategoryCodes.Commerce, SearchText = InfrastructureCategoryCodes.Commerce, Weight = 0.95m, CreatedAtUtc = DateTime.UtcNow },
            new PoiCategory { Id = Guid.NewGuid(), Name = InfrastructureCategoryCodes.Leisure, SearchText = InfrastructureCategoryCodes.Leisure, Weight = 0.85m, CreatedAtUtc = DateTime.UtcNow }
        };

        foreach (var category in categories)
            await poiRepository.AddCategoryAsync(category, cancellationToken);

        await poiRepository.SaveChangesAsync(cancellationToken);
        return categories;
    }
}
