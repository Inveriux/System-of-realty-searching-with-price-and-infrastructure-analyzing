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
                Count = group.Count(),
                Pois = group
                    .OrderBy(static poi => poi.DistanceMeters)
                    .Take(5)
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
            NearbyPois = infrastructure.Pois
                .OrderBy(static x => x.DistanceMeters)
                .Take(12)
                .Select(MapPoi)
                .ToArray(),
            Highlights = infrastructure.Highlights.Count > 0
                ? infrastructure.Highlights.ToArray()
                : BuildInfrastructureHighlights(infrastructure),
            CalculatedAtUtc = infrastructure.Score.CalculatedAtUtc
        };
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

        if (searchFallbackScore is > 0)
            return searchFallbackScore.Value;

        if (infrastructure.Score.PoiCount > 0)
            return Math.Min(90m, 35m + infrastructure.Score.PoiCount * 2m);

        return null;
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
