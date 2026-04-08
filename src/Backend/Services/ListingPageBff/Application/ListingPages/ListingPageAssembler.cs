using System.Text.Json;
using ListingPageBff.Contracts.ListingPages;
using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Application.ListingPages;

public sealed class ListingPageAssembler(TimeProvider timeProvider)
{
    public ListingPageDto Build(
        DownstreamDetailedListingDto listing,
        DownstreamInfrastructureSummaryDto? infrastructure,
        DownstreamSellerPublicProfileDto? seller,
        DownstreamSearchListingDto? searchListing,
        IReadOnlyCollection<ListingPageWarningDto> warnings)
    {
        var infrastructureDto = infrastructure is null ? null : MapInfrastructure(infrastructure, searchListing?.InfrastructureScore);
        var sellerDto = BuildSellerBlock(listing.AgentId, seller);
        var derivedWarnings = warnings.Select(static x => x.Message).ToArray();

        return new ListingPageDto
        {
            Listing = MapListing(listing),
            Infrastructure = infrastructureDto,
            Seller = sellerDto,
            Derived = new ListingPageDerivedDto
            {
                QuickStats = BuildQuickStats(listing),
                FeatureTags = BuildFeatureTags(listing, infrastructure, searchListing, sellerDto),
                Warnings = derivedWarnings,
                LastUpdatedUtc = timeProvider.GetUtcNow()
            },
            AvailableBlocks = new ListingPageAvailableBlocksDto
            {
                Listing = true,
                Infrastructure = infrastructureDto is not null,
                Seller = sellerDto is not null,
                PriceHistory = listing.PriceHistory is { Count: > 0 },
                Map = listing.Latitude.HasValue && listing.Longitude.HasValue
            },
            Warnings = warnings
        };
    }

    public IReadOnlyCollection<ListingQuickStatDto> BuildQuickStats(DownstreamDetailedListingDto listing)
    {
        var stats = new List<ListingQuickStatDto>
        {
            new() { Key = "area", Label = "Площадь", Value = listing.AreaTotal.ToString("0.##"), Unit = "м²" },
            new() { Key = "dealType", Label = "Сделка", Value = TranslateListingType(listing.ListingType) },
            new() { Key = "propertyType", Label = "Тип", Value = TranslateDetailsType(listing.ListingDetailsType) }
        };

        if (!string.IsNullOrWhiteSpace(listing.BuildingType))
            stats.Add(new ListingQuickStatDto { Key = "buildingType", Label = "Здание", Value = TranslateBuildingType(listing.BuildingType!) });

        if (listing.BuildYear is not null)
            stats.Add(new ListingQuickStatDto { Key = "buildYear", Label = "Год постройки", Value = listing.BuildYear.Value.ToString() });

        if (TryGetDetailNumber(listing.ListingDetails, "rooms", out var rooms))
            stats.Add(new ListingQuickStatDto { Key = "rooms", Label = "Комнаты", Value = rooms });

        if (TryGetDetailNumber(listing.ListingDetails, "floor", out var floor))
            stats.Add(new ListingQuickStatDto { Key = "floor", Label = "Этаж", Value = floor });

        if (TryGetDetailNumber(listing.ListingDetails, "floors", out var floors))
            stats.Add(new ListingQuickStatDto { Key = "floors", Label = "Этажей", Value = floors });

        if (TryGetDetailNumber(listing.ListingDetails, "plotArea", out var plotArea))
            stats.Add(new ListingQuickStatDto { Key = "plotArea", Label = "Участок", Value = plotArea, Unit = "м²" });

        return stats;
    }

    public IReadOnlyCollection<ListingFeatureTagDto> BuildFeatureTags(
        DownstreamDetailedListingDto listing,
        DownstreamInfrastructureSummaryDto? infrastructure,
        DownstreamSearchListingDto? searchListing,
        ListingPageSellerDto seller)
    {
        var tags = new List<ListingFeatureTagDto>();

        if (!string.IsNullOrWhiteSpace(listing.BuildingType))
            tags.Add(new ListingFeatureTagDto { Key = "building_type", Label = TranslateBuildingType(listing.BuildingType!), Tone = "neutral" });

        if (seller.IsVerified is true)
            tags.Add(new ListingFeatureTagDto { Key = "verified_seller", Label = "Проверенный продавец", Tone = "positive" });

        if (TryGetDetailBoolean(listing.ListingDetails, "hasGarage", out var hasGarage) && hasGarage)
            tags.Add(new ListingFeatureTagDto { Key = "garage", Label = "Гараж", Tone = "neutral" });

        if (TryGetDetailBoolean(listing.ListingDetails, "hasBalcony", out var hasBalcony) && hasBalcony)
            tags.Add(new ListingFeatureTagDto { Key = "balcony", Label = "Балкон", Tone = "neutral" });

        if (TryGetDetailBoolean(listing.ListingDetails, "hasLoggia", out var hasLoggia) && hasLoggia)
            tags.Add(new ListingFeatureTagDto { Key = "loggia", Label = "Лоджия", Tone = "neutral" });

        var totalInfrastructureScore = infrastructure?.Score?.TotalScore ?? searchListing?.InfrastructureScore;
        if (totalInfrastructureScore is >= 75)
            tags.Add(new ListingFeatureTagDto { Key = "strong_infrastructure", Label = "Сильная инфраструктура", Tone = "positive" });

        if (listing.PriceAmountUsd.HasValue && listing.PriceAmountByn.HasValue)
            tags.Add(new ListingFeatureTagDto { Key = "fx_enriched", Label = "Цена в BYN и USD", Tone = "info" });

        return tags
            .DistinctBy(static x => x.Key)
            .ToArray();
    }

    public IReadOnlyCollection<string> BuildInfrastructureHighlights(DownstreamInfrastructureSummaryDto infrastructure)
    {
        var highlights = new List<string>();
        var score = infrastructure.Score;

        if (score.TotalScore >= 80)
            highlights.Add("Сильная инфраструктура рядом с объектом.");

        if (score.TransportScore >= 75)
            highlights.Add("Хорошая транспортная доступность.");

        if (score.CommerceScore >= 70)
            highlights.Add("Развитая коммерческая инфраструктура.");

        if (score.EducationScore >= 70)
            highlights.Add("Рядом есть образовательные объекты.");

        if (score.HealthcareScore >= 70)
            highlights.Add("Поблизости есть медицинские учреждения.");

        if (highlights.Count == 0 && score.PoiCount > 0)
            highlights.Add($"В радиусе поиска найдено {score.PoiCount} объектов инфраструктуры.");

        return highlights;
    }

    public IReadOnlyCollection<ListingPageGroupedPoisDto> GroupPoisByCategory(IReadOnlyCollection<DownstreamPoiDto> pois)
    {
        return pois
            .GroupBy(poi => TranslatePoiCategory(string.IsNullOrWhiteSpace(poi.Category) ? "other" : poi.Category))
            .OrderByDescending(static group => group.Count())
            .Select(group => new ListingPageGroupedPoisDto
            {
                Category = group.Key,
                Count = group.Key.Equals("Досуг", StringComparison.OrdinalIgnoreCase)
                    ? DeduplicateLeisurePois(group).Count
                    : group.Count(),
                Pois = (group.Key.Equals("Досуг", StringComparison.OrdinalIgnoreCase)
                        ? DeduplicateLeisurePois(group)
                        : group.OrderBy(static poi => poi.DistanceMeters).ToList())
                    .Select(MapPoi)
                    .ToArray()
            })
            .ToArray();
    }

    private ListingPageListingDto MapListing(DownstreamDetailedListingDto listing)
    {
        return new ListingPageListingDto
        {
            Id = listing.Id,
            AgentId = listing.AgentId,
            LocationId = listing.LocationId,
            Title = listing.Title,
            Description = listing.Description,
            AddressText = listing.AddressText,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            PriceAmount = listing.PriceAmount,
            PriceCurrency = listing.PriceCurrency,
            PriceAmountByn = listing.PriceAmountByn,
            PriceAmountUsd = listing.PriceAmountUsd,
            PricePeriod = listing.PricePeriod,
            AreaTotal = listing.AreaTotal,
            BuildYear = listing.BuildYear,
            PublishedDate = listing.PublishedDate,
            BuildingType = listing.BuildingType,
            ListingType = listing.ListingType,
            MainMedia = listing.MainMedia,
            Media = listing.Media.Select(static x => new ListingPageMediaDto { Url = x.Url }).ToArray(),
            ListingDetailsType = listing.ListingDetailsType,
            ListingDetails = listing.ListingDetails,
            PriceHistory = listing.PriceHistory.Select(static x => new ListingPagePriceHistoryDto
            {
                ChangedAt = x.ChangedAt,
                PriceAmount = x.PriceAmount,
                PriceCurrency = x.PriceCurrency,
                PriceAmountByn = x.PriceAmountByn,
                PriceAmountUsd = x.PriceAmountUsd
            }).ToArray()
        };
    }

    private ListingPageInfrastructureDto MapInfrastructure(
        DownstreamInfrastructureSummaryDto infrastructure,
        decimal? searchFallbackScore)
    {
        var resolvedTotalScore = ResolveInfrastructureScore(infrastructure, searchFallbackScore);
        var rawMapPois = infrastructure.Pois
            .OrderBy(static x => x.DistanceMeters)
            .Select(MapPoi)
            .ToArray();
        var groupedTransportSummary = BuildGroupedTransportSummary(infrastructure);
        var compactInfrastructureSummary = BuildCompactInfrastructureSummary(infrastructure, groupedTransportSummary);

        return new ListingPageInfrastructureDto
        {
            Score = resolvedTotalScore,
            TotalScore = resolvedTotalScore,
            EducationScore = infrastructure.Score.EducationScore,
            HealthcareScore = infrastructure.Score.HealthcareScore,
            TransportScore = infrastructure.Score.TransportScore,
            CommerceScore = infrastructure.Score.CommerceScore,
            LeisureScore = infrastructure.Score.LeisureScore,
            PoiCount = infrastructure.Score.PoiCount,
            Summary = string.IsNullOrWhiteSpace(infrastructure.Summary)
                ? BuildInfrastructureSummaryText(infrastructure)
                : infrastructure.Summary,
            GroupedPois = GroupPoisByCategory(infrastructure.Pois),
            NearbyPois = rawMapPois,
            RawMapPois = rawMapPois,
            NearbyCategories = infrastructure.NearbyCategories
                .Select(static x => new ListingPageInfrastructureNearbyCategoryDto
                {
                    CategoryCode = x.CategoryCode,
                    SubcategoryCode = x.SubcategoryCode,
                    CountInPrimaryRadius = x.CountInPrimaryRadius,
                    CandidateCount = x.CandidateCount,
                    ReachableWithin10MinCount = x.ReachableWithin10MinCount,
                    ReachableWithin15MinCount = x.ReachableWithin15MinCount,
                    NearestDistanceM = x.NearestDistanceM,
                    RouteDistanceMeters = x.RouteDistanceMeters,
                    TravelDurationSeconds = x.TravelDurationSeconds,
                    WalkingMinutes = x.WalkingMinutes,
                    NearestWalkMinutes = x.NearestWalkMinutes,
                    NearestDriveMinutes = x.NearestDriveMinutes,
                    NearestDriveMinutesEstimated = x.NearestDriveMinutesEstimated,
                    NearestAnyDistanceM = x.NearestAnyDistanceM,
                    NearestAnyWalkMinutes = x.NearestAnyWalkMinutes,
                    NearestAnyDriveMinutes = x.NearestAnyDriveMinutes,
                    NearestAnyDriveMinutesEstimated = x.NearestAnyDriveMinutesEstimated,
                    UsedRouting = x.UsedRouting,
                    FallbackMode = x.FallbackMode,
                    FoundInRadiusM = x.FoundInRadiusM,
                    FoundInPrimaryRadius = x.FoundInPrimaryRadius
                })
                .ToArray(),
            TransportDetails = new ListingPageInfrastructureTransportDetailsDto
            {
                NearestStopDistanceM = infrastructure.TransportDetails.NearestStopDistanceM,
                NearestStopWalkMinutes = infrastructure.TransportDetails.NearestStopWalkMinutes,
                StopsCountPrimary = infrastructure.TransportDetails.StopsCountPrimary,
                StopGroupsCountPrimary = infrastructure.TransportDetails.StopGroupsCountPrimary,
                StopPlatformsCountPrimary = infrastructure.TransportDetails.StopPlatformsCountPrimary,
                NearestStopGroupDistanceM = infrastructure.TransportDetails.NearestStopGroupDistanceM,
                NearestStopGroupWalkMinutes = infrastructure.TransportDetails.NearestStopGroupWalkMinutes,
                NearestStopGroupDriveMinutes = infrastructure.TransportDetails.NearestStopGroupDriveMinutes,
                NearestStopGroupDriveMinutesEstimated = infrastructure.TransportDetails.NearestStopGroupDriveMinutesEstimated,
                NearestStopGroupName = infrastructure.TransportDetails.NearestStopGroupName,
                NearestStopGroupPlatformCount = infrastructure.TransportDetails.NearestStopGroupPlatformCount,
                NearestStopGroupAnyDistanceM = infrastructure.TransportDetails.NearestStopGroupAnyDistanceM,
                NearestStopGroupAnyWalkMinutes = infrastructure.TransportDetails.NearestStopGroupAnyWalkMinutes,
                NearestStopGroupAnyDriveMinutes = infrastructure.TransportDetails.NearestStopGroupAnyDriveMinutes,
                NearestStopGroupAnyDriveMinutesEstimated = infrastructure.TransportDetails.NearestStopGroupAnyDriveMinutesEstimated,
                NearestStopGroupFoundRadiusM = infrastructure.TransportDetails.NearestStopGroupFoundRadiusM,
                StopGroups = infrastructure.TransportDetails.StopGroups
                    .Select(static group => new ListingPageInfrastructureTransportStopGroupDto
                    {
                        GroupId = group.GroupId,
                        NormalizedName = group.NormalizedName,
                        DisplayName = group.DisplayName,
                        CenterLatitude = group.CenterLatitude,
                        CenterLongitude = group.CenterLongitude,
                        MaxDistanceWithinGroup = group.MaxDistanceWithinGroup,
                        PointCount = group.PointCount,
                        PlatformCount = group.PlatformCount,
                        StopTypes = group.StopTypes.ToArray(),
                        SourcePoiIds = group.SourcePoiIds.Select(static id => id.ToString()).ToArray(),
                        NearestDistanceMeters = group.NearestDistanceMeters,
                        RouteDistanceMeters = group.RouteDistanceMeters,
                        WalkMinutes = group.WalkMinutes,
                        UsedRouting = group.UsedRouting,
                        FallbackMode = group.FallbackMode
                    })
                    .ToArray(),
                NearestMetroDistanceM = infrastructure.TransportDetails.NearestMetroDistanceM,
                NearestMetroWalkMinutes = infrastructure.TransportDetails.NearestMetroWalkMinutes,
                NearestMetroName = infrastructure.TransportDetails.NearestMetroName,
                MetroFoundInPrimaryRadius = infrastructure.TransportDetails.MetroFoundInPrimaryRadius,
                NearestParkingDistanceM = infrastructure.TransportDetails.NearestParkingDistanceM,
                NearestParkingWalkMinutes = infrastructure.TransportDetails.NearestParkingWalkMinutes,
                NearestParkingDriveMinutes = infrastructure.TransportDetails.NearestParkingDriveMinutes,
                NearestParkingDriveMinutesEstimated = infrastructure.TransportDetails.NearestParkingDriveMinutesEstimated,
                ParkingCountPrimary = infrastructure.TransportDetails.ParkingCountPrimary
            },
            GroupedTransportSummary = groupedTransportSummary,
            CompactInfrastructureSummary = compactInfrastructureSummary,
            SearchFeatures = new ListingPageInfrastructureSearchFeaturesDto
            {
                StoreCountPrimary = infrastructure.SearchFeatures.StoreCountPrimary,
                NearestStoreWalkMinutes = infrastructure.SearchFeatures.NearestStoreWalkMinutes,
                NearestStoreDriveMinutes = infrastructure.SearchFeatures.NearestStoreDriveMinutes,
                NearestStoreDriveMinutesEstimated = infrastructure.SearchFeatures.NearestStoreDriveMinutesEstimated,
                NearestStoreAnyWalkMinutes = infrastructure.SearchFeatures.NearestStoreAnyWalkMinutes,
                NearestStoreAnyDriveMinutes = infrastructure.SearchFeatures.NearestStoreAnyDriveMinutes,
                NearestStoreAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestStoreAnyDriveMinutesEstimated,
                NearestStoreFoundRadiusM = infrastructure.SearchFeatures.NearestStoreFoundRadiusM,
                PharmacyCountPrimary = infrastructure.SearchFeatures.PharmacyCountPrimary,
                NearestPharmacyWalkMinutes = infrastructure.SearchFeatures.NearestPharmacyWalkMinutes,
                NearestPharmacyDriveMinutes = infrastructure.SearchFeatures.NearestPharmacyDriveMinutes,
                NearestPharmacyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestPharmacyDriveMinutesEstimated,
                NearestPharmacyAnyWalkMinutes = infrastructure.SearchFeatures.NearestPharmacyAnyWalkMinutes,
                NearestPharmacyAnyDriveMinutes = infrastructure.SearchFeatures.NearestPharmacyAnyDriveMinutes,
                NearestPharmacyAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestPharmacyAnyDriveMinutesEstimated,
                NearestPharmacyFoundRadiusM = infrastructure.SearchFeatures.NearestPharmacyFoundRadiusM,
                StopsCountPrimary = infrastructure.SearchFeatures.StopsCountPrimary,
                NearestStopWalkMinutes = infrastructure.SearchFeatures.NearestStopWalkMinutes,
                NearestStopDriveMinutes = infrastructure.SearchFeatures.NearestStopDriveMinutes,
                NearestStopDriveMinutesEstimated = infrastructure.SearchFeatures.NearestStopDriveMinutesEstimated,
                NearestStopAnyWalkMinutes = infrastructure.SearchFeatures.NearestStopAnyWalkMinutes,
                NearestStopAnyDriveMinutes = infrastructure.SearchFeatures.NearestStopAnyDriveMinutes,
                NearestStopAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestStopAnyDriveMinutesEstimated,
                NearestStopFoundRadiusM = infrastructure.SearchFeatures.NearestStopFoundRadiusM,
                StopGroupsCountPrimary = infrastructure.SearchFeatures.StopGroupsCountPrimary,
                StopPlatformsCountPrimary = infrastructure.SearchFeatures.StopPlatformsCountPrimary,
                NearestStopGroupWalkMinutes = infrastructure.SearchFeatures.NearestStopGroupWalkMinutes,
                NearestStopGroupDriveMinutes = infrastructure.SearchFeatures.NearestStopGroupDriveMinutes,
                NearestStopGroupDriveMinutesEstimated = infrastructure.SearchFeatures.NearestStopGroupDriveMinutesEstimated,
                NearestStopGroupName = infrastructure.SearchFeatures.NearestStopGroupName,
                NearestStopGroupPlatformCount = infrastructure.SearchFeatures.NearestStopGroupPlatformCount,
                NearestStopGroupAnyWalkMinutes = infrastructure.SearchFeatures.NearestStopGroupAnyWalkMinutes,
                NearestStopGroupAnyDriveMinutes = infrastructure.SearchFeatures.NearestStopGroupAnyDriveMinutes,
                NearestStopGroupAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestStopGroupAnyDriveMinutesEstimated,
                NearestStopGroupFoundRadiusM = infrastructure.SearchFeatures.NearestStopGroupFoundRadiusM,
                ParkingCountPrimary = infrastructure.SearchFeatures.ParkingCountPrimary,
                NearestParkingWalkMinutes = infrastructure.SearchFeatures.NearestParkingWalkMinutes,
                NearestParkingDriveMinutes = infrastructure.SearchFeatures.NearestParkingDriveMinutes,
                NearestParkingDriveMinutesEstimated = infrastructure.SearchFeatures.NearestParkingDriveMinutesEstimated,
                NearestParkingAnyWalkMinutes = infrastructure.SearchFeatures.NearestParkingAnyWalkMinutes,
                NearestParkingAnyDriveMinutes = infrastructure.SearchFeatures.NearestParkingAnyDriveMinutes,
                NearestParkingAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestParkingAnyDriveMinutesEstimated,
                NearestParkingFoundRadiusM = infrastructure.SearchFeatures.NearestParkingFoundRadiusM,
                SchoolCountPrimary = infrastructure.SearchFeatures.SchoolCountPrimary,
                NearestSchoolWalkMinutes = infrastructure.SearchFeatures.NearestSchoolWalkMinutes,
                NearestSchoolDriveMinutes = infrastructure.SearchFeatures.NearestSchoolDriveMinutes,
                NearestSchoolDriveMinutesEstimated = infrastructure.SearchFeatures.NearestSchoolDriveMinutesEstimated,
                NearestSchoolAnyWalkMinutes = infrastructure.SearchFeatures.NearestSchoolAnyWalkMinutes,
                NearestSchoolAnyDriveMinutes = infrastructure.SearchFeatures.NearestSchoolAnyDriveMinutes,
                NearestSchoolAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestSchoolAnyDriveMinutesEstimated,
                NearestSchoolFoundRadiusM = infrastructure.SearchFeatures.NearestSchoolFoundRadiusM,
                KindergartenCountPrimary = infrastructure.SearchFeatures.KindergartenCountPrimary,
                NearestKindergartenWalkMinutes = infrastructure.SearchFeatures.NearestKindergartenWalkMinutes,
                NearestKindergartenDriveMinutes = infrastructure.SearchFeatures.NearestKindergartenDriveMinutes,
                NearestKindergartenDriveMinutesEstimated = infrastructure.SearchFeatures.NearestKindergartenDriveMinutesEstimated,
                NearestKindergartenAnyWalkMinutes = infrastructure.SearchFeatures.NearestKindergartenAnyWalkMinutes,
                NearestKindergartenAnyDriveMinutes = infrastructure.SearchFeatures.NearestKindergartenAnyDriveMinutes,
                NearestKindergartenAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestKindergartenAnyDriveMinutesEstimated,
                NearestKindergartenFoundRadiusM = infrastructure.SearchFeatures.NearestKindergartenFoundRadiusM,
                ClinicCountPrimary = infrastructure.SearchFeatures.ClinicCountPrimary,
                NearestClinicWalkMinutes = infrastructure.SearchFeatures.NearestClinicWalkMinutes,
                NearestClinicDriveMinutes = infrastructure.SearchFeatures.NearestClinicDriveMinutes,
                NearestClinicDriveMinutesEstimated = infrastructure.SearchFeatures.NearestClinicDriveMinutesEstimated,
                NearestClinicAnyDriveMinutes = infrastructure.SearchFeatures.NearestClinicAnyDriveMinutes,
                NearestClinicAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestClinicAnyDriveMinutesEstimated,
                NearestClinicFoundRadiusM = infrastructure.SearchFeatures.NearestClinicFoundRadiusM,
                HospitalCountPrimary = infrastructure.SearchFeatures.HospitalCountPrimary,
                NearestHospitalDriveMinutes = infrastructure.SearchFeatures.NearestHospitalDriveMinutes,
                NearestHospitalDriveMinutesEstimated = infrastructure.SearchFeatures.NearestHospitalDriveMinutesEstimated,
                NearestHospitalAnyDriveMinutes = infrastructure.SearchFeatures.NearestHospitalAnyDriveMinutes,
                NearestHospitalAnyDriveMinutesEstimated = infrastructure.SearchFeatures.NearestHospitalAnyDriveMinutesEstimated,
                NearestHospitalFoundRadiusM = infrastructure.SearchFeatures.NearestHospitalFoundRadiusM,
                LeisureCountPrimary = infrastructure.SearchFeatures.LeisureCountPrimary,
                NearestLeisureWalkMinutes = infrastructure.SearchFeatures.NearestLeisureWalkMinutes,
                NearestLeisureAnyWalkMinutes = infrastructure.SearchFeatures.NearestLeisureAnyWalkMinutes,
                NearestLeisureFoundRadiusM = infrastructure.SearchFeatures.NearestLeisureFoundRadiusM,
                ParksCountPrimary = infrastructure.SearchFeatures.ParksCountPrimary,
                NearestParkWalkMinutes = infrastructure.SearchFeatures.NearestParkWalkMinutes,
                NearestParkDriveMinutes = infrastructure.SearchFeatures.NearestParkDriveMinutes,
                NearestParkAnyWalkMinutes = infrastructure.SearchFeatures.NearestParkAnyWalkMinutes,
                NearestParkAnyDriveMinutes = infrastructure.SearchFeatures.NearestParkAnyDriveMinutes,
                NearestParkFoundRadiusM = infrastructure.SearchFeatures.NearestParkFoundRadiusM,
                FitnessCountPrimary = infrastructure.SearchFeatures.FitnessCountPrimary,
                NearestFitnessWalkMinutes = infrastructure.SearchFeatures.NearestFitnessWalkMinutes,
                NearestFitnessDriveMinutes = infrastructure.SearchFeatures.NearestFitnessDriveMinutes,
                NearestFitnessAnyWalkMinutes = infrastructure.SearchFeatures.NearestFitnessAnyWalkMinutes,
                NearestFitnessAnyDriveMinutes = infrastructure.SearchFeatures.NearestFitnessAnyDriveMinutes,
                NearestFitnessFoundRadiusM = infrastructure.SearchFeatures.NearestFitnessFoundRadiusM,
                MuseumsCountPrimary = infrastructure.SearchFeatures.MuseumsCountPrimary,
                NearestMuseumWalkMinutes = infrastructure.SearchFeatures.NearestMuseumWalkMinutes,
                NearestMuseumDriveMinutes = infrastructure.SearchFeatures.NearestMuseumDriveMinutes,
                NearestMuseumAnyWalkMinutes = infrastructure.SearchFeatures.NearestMuseumAnyWalkMinutes,
                NearestMuseumAnyDriveMinutes = infrastructure.SearchFeatures.NearestMuseumAnyDriveMinutes,
                NearestMuseumFoundRadiusM = infrastructure.SearchFeatures.NearestMuseumFoundRadiusM,
                TheatersCountPrimary = infrastructure.SearchFeatures.TheatersCountPrimary,
                NearestTheaterWalkMinutes = infrastructure.SearchFeatures.NearestTheaterWalkMinutes,
                NearestTheaterDriveMinutes = infrastructure.SearchFeatures.NearestTheaterDriveMinutes,
                NearestTheaterAnyWalkMinutes = infrastructure.SearchFeatures.NearestTheaterAnyWalkMinutes,
                NearestTheaterAnyDriveMinutes = infrastructure.SearchFeatures.NearestTheaterAnyDriveMinutes,
                NearestTheaterFoundRadiusM = infrastructure.SearchFeatures.NearestTheaterFoundRadiusM,
                CinemasCountPrimary = infrastructure.SearchFeatures.CinemasCountPrimary,
                NearestCinemaWalkMinutes = infrastructure.SearchFeatures.NearestCinemaWalkMinutes,
                NearestCinemaDriveMinutes = infrastructure.SearchFeatures.NearestCinemaDriveMinutes,
                NearestCinemaAnyWalkMinutes = infrastructure.SearchFeatures.NearestCinemaAnyWalkMinutes,
                NearestCinemaAnyDriveMinutes = infrastructure.SearchFeatures.NearestCinemaAnyDriveMinutes,
                NearestCinemaFoundRadiusM = infrastructure.SearchFeatures.NearestCinemaFoundRadiusM,
                CafesCountPrimary = infrastructure.SearchFeatures.CafesCountPrimary,
                RestaurantsCountPrimary = infrastructure.SearchFeatures.RestaurantsCountPrimary,
                NearestMetroWalkMinutes = infrastructure.SearchFeatures.NearestMetroWalkMinutes,
                NearestMetroName = infrastructure.SearchFeatures.NearestMetroName,
                MetroFoundInPrimaryRadius = infrastructure.SearchFeatures.MetroFoundInPrimaryRadius
            },
            MissingPrimaryCategories = infrastructure.MissingPrimaryCategories.ToArray(),
            Highlights = infrastructure.Highlights.Count > 0
                ? infrastructure.Highlights.ToArray()
                : BuildInfrastructureHighlights(infrastructure),
            CalculatedAtUtc = infrastructure.Score.CalculatedAtUtc
        };
    }

    private static ListingPageGroupedTransportSummaryDto BuildGroupedTransportSummary(DownstreamInfrastructureSummaryDto infrastructure)
    {
        var details = infrastructure.TransportDetails;
        var parking = infrastructure.NearbyCategories.FirstOrDefault(static x => x.SubcategoryCode == "parking");

        return new ListingPageGroupedTransportSummaryDto
        {
            NearestStopGroupName = details.NearestStopGroupName,
            NearestStopGroupDistanceM = details.NearestStopGroupDistanceM,
            NearestStopGroupWalkMinutes = details.NearestStopGroupWalkMinutes,
            NearestStopGroupDriveMinutes = details.NearestStopGroupDriveMinutes,
            NearestStopGroupDriveMinutesEstimated = details.NearestStopGroupDriveMinutesEstimated,
            NearestStopGroupAnyDistanceM = details.NearestStopGroupAnyDistanceM,
            NearestStopGroupAnyWalkMinutes = details.NearestStopGroupAnyWalkMinutes,
            NearestStopGroupAnyDriveMinutes = details.NearestStopGroupAnyDriveMinutes,
            NearestStopGroupAnyDriveMinutesEstimated = details.NearestStopGroupAnyDriveMinutesEstimated,
            StopGroupFoundInPrimaryRadius = details.NearestStopGroupWalkMinutes is not null || details.NearestStopGroupDistanceM is not null,
            NearestStopGroupFoundRadiusM = details.NearestStopGroupFoundRadiusM,
            StopGroupsCountPrimary = details.StopGroupsCountPrimary,
            StopPlatformsCountPrimary = details.StopPlatformsCountPrimary,
            NearestStopGroupPlatformCount = details.NearestStopGroupPlatformCount,
            NearestMetroDistanceM = details.NearestMetroDistanceM,
            NearestMetroWalkMinutes = details.NearestMetroWalkMinutes,
            NearestMetroName = details.NearestMetroName,
            MetroFoundInPrimaryRadius = details.MetroFoundInPrimaryRadius,
            NearestParkingDistanceM = details.NearestParkingDistanceM,
            NearestParkingWalkMinutes = details.NearestParkingWalkMinutes,
            NearestParkingDriveMinutes = details.NearestParkingDriveMinutes,
            NearestParkingDriveMinutesEstimated = details.NearestParkingDriveMinutesEstimated,
            NearestParkingAnyDistanceM = parking?.NearestAnyDistanceM,
            NearestParkingAnyWalkMinutes = parking?.NearestAnyWalkMinutes,
            NearestParkingAnyDriveMinutes = parking?.NearestAnyDriveMinutes,
            NearestParkingAnyDriveMinutesEstimated = parking?.NearestAnyDriveMinutesEstimated ?? false,
            ParkingFoundInPrimaryRadius = parking?.FoundInPrimaryRadius ?? details.NearestParkingWalkMinutes is not null,
            NearestParkingFoundRadiusM = parking?.FoundInRadiusM
        };
    }

    private static IReadOnlyCollection<ListingPageCompactInfrastructureItemDto> BuildCompactInfrastructureSummary(
        DownstreamInfrastructureSummaryDto infrastructure,
        ListingPageGroupedTransportSummaryDto groupedTransportSummary)
    {
        return
        [
            new ListingPageCompactInfrastructureItemDto
            {
                Key = "stop_group",
                Label = "Ближайшая остановка",
                IconSrc = "/map-markers/bus-stop.svg",
                EntityName = groupedTransportSummary.NearestStopGroupName,
                PrimaryDistanceM = groupedTransportSummary.NearestStopGroupDistanceM,
                PrimaryWalkMinutes = groupedTransportSummary.NearestStopGroupWalkMinutes,
                PrimaryDriveMinutes = groupedTransportSummary.NearestStopGroupDriveMinutes,
                PrimaryDriveMinutesEstimated = groupedTransportSummary.NearestStopGroupDriveMinutesEstimated,
                FallbackDistanceM = groupedTransportSummary.NearestStopGroupAnyDistanceM,
                FallbackWalkMinutes = groupedTransportSummary.NearestStopGroupAnyWalkMinutes,
                FallbackDriveMinutes = groupedTransportSummary.NearestStopGroupAnyDriveMinutes,
                FallbackDriveMinutesEstimated = groupedTransportSummary.NearestStopGroupAnyDriveMinutesEstimated,
                FoundInPrimaryRadius = groupedTransportSummary.StopGroupFoundInPrimaryRadius,
                FoundInRadiusM = groupedTransportSummary.NearestStopGroupFoundRadiusM,
                Hint = groupedTransportSummary.NearestStopGroupPlatformCount is > 1
                    ? $"Платформ в ближайшей группе: {groupedTransportSummary.NearestStopGroupPlatformCount}"
                    : groupedTransportSummary.StopGroupsCountPrimary > 0
                        ? $"Остановочных групп рядом: {groupedTransportSummary.StopGroupsCountPrimary}"
                        : "В основной зоне доступности не найдено"
            },
            BuildCompactSummaryItem(infrastructure, "parking", "Ближайшая парковка", "/map-markers/parking.svg"),
            BuildCompactSummaryItem(infrastructure, "store", "Ближайший магазин", "/map-markers/supermarket.svg"),
            BuildCompactSummaryItem(infrastructure, "pharmacy", "Ближайшая аптека", "/map-markers/pharmacy.svg"),
            BuildCompactSummaryItem(infrastructure, "school", "Ближайшая школа", "/map-markers/school.svg"),
            BuildCompactSummaryItem(infrastructure, "kindergarten", "Ближайший детский сад", "/map-markers/kindergarten.svg"),
            BuildCompactSummaryItem(infrastructure, "clinic", "Ближайшая поликлиника", "/map-markers/healthcare.svg"),
            BuildCompactSummaryItem(infrastructure, "hospital", "Ближайшая больница", "/map-markers/hospital.svg"),
            BuildCompactSummaryItem(infrastructure, "park", "Парк", "/map-markers/park.svg"),
            BuildCompactSummaryItem(infrastructure, "fitness", "Фитнес-клуб", "/map-markers/fitness.svg"),
            BuildCompactSummaryItem(infrastructure, "museum", "Музей", "/map-markers/museum.svg"),
            BuildCompactSummaryItem(infrastructure, "theater", "Театр", "/map-markers/theater.svg"),
            BuildCompactSummaryItem(infrastructure, "cinema", "Кинотеатр", "/map-markers/cinema.svg"),
            new ListingPageCompactInfrastructureItemDto
            {
                Key = "metro",
                Label = "Ближайшая станция метро",
                IconSrc = "/map-markers/minsk-metro.svg",
                EntityName = infrastructure.TransportDetails.NearestMetroName,
                PrimaryDistanceM = infrastructure.TransportDetails.NearestMetroDistanceM,
                PrimaryWalkMinutes = infrastructure.TransportDetails.NearestMetroWalkMinutes,
                PrimaryDriveMinutes = null,
                PrimaryDriveMinutesEstimated = false,
                FallbackDistanceM = null,
                FallbackWalkMinutes = null,
                FallbackDriveMinutes = null,
                FallbackDriveMinutesEstimated = false,
                FoundInPrimaryRadius = infrastructure.TransportDetails.MetroFoundInPrimaryRadius,
                FoundInRadiusM = infrastructure.TransportDetails.MetroFoundInPrimaryRadius ? 3000 : null,
                Hint = infrastructure.TransportDetails.MetroFoundInPrimaryRadius
                    ? "Метро в основной зоне доступности"
                    : "Станции метро в пределах заданного радиуса не обнаружены."
            }
        ];
    }

    private static List<DownstreamPoiDto> DeduplicateLeisurePois(IEnumerable<DownstreamPoiDto> pois)
        => pois
            .OrderBy(static poi => poi.DistanceMeters)
            .GroupBy(static poi =>
                $"{poi.SubcategoryCode?.Trim().ToLowerInvariant() ?? string.Empty}:{poi.Name.Trim().ToLowerInvariant()}",
                StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();

    private static ListingPageCompactInfrastructureItemDto BuildCompactSummaryItem(
        DownstreamInfrastructureSummaryDto infrastructure,
        string subcategoryCode,
        string label,
        string iconSrc)
    {
        var item = ResolveCompactSummaryCategory(infrastructure, subcategoryCode);

        return new ListingPageCompactInfrastructureItemDto
        {
            Key = subcategoryCode,
            Label = label,
            IconSrc = iconSrc,
            EntityName = null,
            PrimaryDistanceM = item?.NearestDistanceM,
            PrimaryWalkMinutes = item?.NearestWalkMinutes,
            PrimaryDriveMinutes = item?.NearestDriveMinutes,
            PrimaryDriveMinutesEstimated = item?.NearestDriveMinutesEstimated ?? false,
            FallbackDistanceM = item?.NearestAnyDistanceM,
            FallbackWalkMinutes = item?.NearestAnyWalkMinutes,
            FallbackDriveMinutes = item?.NearestAnyDriveMinutes,
            FallbackDriveMinutesEstimated = item?.NearestAnyDriveMinutesEstimated ?? false,
            FoundInPrimaryRadius = item?.FoundInPrimaryRadius ?? false,
            FoundInRadiusM = item?.FoundInRadiusM,
            Hint = item?.FoundInPrimaryRadius == true
                ? null
                : item?.NearestAnyDistanceM is not null || item?.NearestAnyWalkMinutes is not null || item?.NearestAnyDriveMinutes is not null
                    ? "В основной зоне доступности не найдено"
                    : "В доступном радиусе не найдено"
        };
    }

    private static DownstreamInfrastructureNearbyCategoryDto? ResolveCompactSummaryCategory(
        DownstreamInfrastructureSummaryDto infrastructure,
        string subcategoryCode)
    {
        if (!subcategoryCode.Equals("store", StringComparison.OrdinalIgnoreCase))
            return infrastructure.NearbyCategories.FirstOrDefault(x => x.SubcategoryCode == subcategoryCode);

        return infrastructure.NearbyCategories
            .Where(x => x.SubcategoryCode is "supermarket" or "convenience_store" or "hypermarket" or "discount_store")
            .OrderBy(x => x.WalkingMinutes ?? x.NearestWalkMinutes ?? decimal.MaxValue)
            .ThenBy(x => x.RouteDistanceMeters ?? x.NearestDistanceM ?? decimal.MaxValue)
            .FirstOrDefault();
    }

    private static decimal? ResolveInfrastructureScore(
        DownstreamInfrastructureSummaryDto infrastructure,
        decimal? searchFallbackScore)
    {
        if (infrastructure.Score.TotalScore > 0)
            return infrastructure.Score.TotalScore;

        var componentScores = new[]
        {
            infrastructure.Score.EducationScore,
            infrastructure.Score.HealthcareScore,
            infrastructure.Score.TransportScore,
            infrastructure.Score.CommerceScore,
            infrastructure.Score.LeisureScore
        }
        .Where(static score => score > 0)
        .ToArray();

        if (componentScores.Length > 0)
            return Math.Round(componentScores.Average(), 1);

        if (searchFallbackScore is > 0 && infrastructure.Score.PoiCount <= 0 && infrastructure.Pois.Count == 0)
            return searchFallbackScore.Value;

        return infrastructure.Score.TotalScore;
    }

    private ListingPagePoiDto MapPoi(DownstreamPoiDto poi) => new()
    {
        Id = poi.Id,
        ExternalId = poi.ExternalId,
        Name = SanitizePoiName(poi.Name, poi.Category),
        AddressText = poi.AddressText,
        Latitude = poi.Latitude,
        Longitude = poi.Longitude,
        Category = TranslatePoiCategory(poi.Category),
        SubcategoryCode = poi.SubcategoryCode,
        DistanceMeters = poi.DistanceMeters
    };

    private ListingPageSellerDto BuildSellerBlock(Guid agentId, DownstreamSellerPublicProfileDto? seller)
    {
        if (seller is null)
        {
            return new ListingPageSellerDto
            {
                Id = agentId,
                FullName = "Контакты продавца уточняются",
                IsFallback = true
            };
        }

        return new ListingPageSellerDto
        {
            Id = seller.Id,
            FullName = seller.FullName,
            AvatarUrl = seller.AvatarUrl,
            Phone = seller.Phone,
            Email = seller.Email,
            Rating = seller.Rating,
            ReviewsCount = seller.ReviewsCount,
            IsVerified = seller.IsVerified,
            AgencyName = seller.AgencyName,
            IsFallback = false
        };
    }

    private static string BuildInfrastructureSummaryText(DownstreamInfrastructureSummaryDto infrastructure)
    {
        var nearest = infrastructure.Pois
            .OrderBy(static x => x.DistanceMeters)
            .Take(3)
            .Select(x => $"{SanitizePoiName(x.Name, x.Category)} ({x.DistanceMeters:0} м)")
            .ToArray();

        return nearest.Length == 0
            ? "Инфраструктурная сводка доступна, но ближайшие POI не найдены."
            : $"Ближайшие объекты: {string.Join(", ", nearest)}.";
    }

    private static string SanitizePoiName(string? name, string? category)
    {
        if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name, "POI", StringComparison.OrdinalIgnoreCase))
            return name!;

        return category?.ToLowerInvariant() switch
        {
            "transport" => "Транспортный узел",
            "education" => "Образовательный объект",
            "healthcare" => "Медицинский объект",
            "commerce" => "Торговый объект",
            "leisure" => "Объект досуга",
            _ => "Инфраструктурный объект"
        };
    }

    private static string TranslatePoiCategory(string? category)
        => category?.ToLowerInvariant() switch
        {
            "education" => "Образование",
            "healthcare" => "Здоровье",
            "transport" => "Транспорт",
            "commerce" => "Коммерция",
            "leisure" => "Досуг",
            _ => "Инфраструктура"
        };

    private static string TranslateListingType(string? listingType)
        => listingType?.ToLowerInvariant() switch
        {
            "sale" => "Продажа",
            "rent" => "Аренда",
            _ => listingType ?? "Не указано"
        };

    private static string TranslateDetailsType(string? detailsType)
        => detailsType?.ToLowerInvariant() switch
        {
            "apartment" => "Квартира",
            "house" => "Дом",
            "land" => "Участок",
            "office" => "Офис",
            "warehouse" => "Склад",
            _ => detailsType ?? "Не указано"
        };

    private static string TranslateBuildingType(string value)
        => value.ToLowerInvariant() switch
        {
            "other" => "Другое",
            "detached house" => "Отдельный дом",
            "townhouse" => "Таунхаус",
            "business center" => "Бизнес-центр",
            "warehouse complex" => "Складской комплекс",
            "industrial building" => "Промышленное здание",
            "shopping mall" => "Торговый центр",
            "mixed-use building" => "Многофункциональное здание",
            "apartment building (multi-family)" => "Многоквартирный дом",
            _ => value
        };

    private static bool TryGetDetailBoolean(JsonElement? listingDetails, string propertyName, out bool value)
    {
        value = false;
        if (listingDetails is not { ValueKind: JsonValueKind.Object } details)
            return false;

        if (!details.TryGetProperty(propertyName, out var property) || property.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
            return false;

        value = property.GetBoolean();
        return true;
    }

    private static bool TryGetDetailNumber(JsonElement? listingDetails, string propertyName, out string value)
    {
        value = string.Empty;
        if (listingDetails is not { ValueKind: JsonValueKind.Object } details)
            return false;

        if (!details.TryGetProperty(propertyName, out var property))
            return false;

        if (property.ValueKind == JsonValueKind.Number)
        {
            value = property.ToString();
            return true;
        }

        return false;
    }
}
