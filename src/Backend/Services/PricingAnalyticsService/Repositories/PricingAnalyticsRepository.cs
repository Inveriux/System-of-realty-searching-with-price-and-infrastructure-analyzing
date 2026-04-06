using Microsoft.EntityFrameworkCore;
using PricingAnalyticsService.Data;
using PricingAnalyticsService.DTOs;
using PricingAnalyticsService.Models;
using PricingAnalyticsService.Repositories.Abstractions;

namespace PricingAnalyticsService.Repositories;

public sealed class PricingAnalyticsRepository(ApplicationDbContext context) : IPricingAnalyticsRepository
{
    private const int DefaultMonths = 12;
    private const int MinimumGeneralSampleSize = 3;
    private const int MinimumReliableLocalSampleSize = 5;

    public async Task<PricingAnalyticsSummaryDto> GetSummaryAsync(PricingAnalyticsQueryDto query, CancellationToken cancellationToken)
    {
        var samples = await LoadFilteredSamplesAsync(query, cancellationToken);
        var period = ResolvePeriod(samples);
        var metrics = BuildMetrics(samples.Select(x => x.PricePerSqmByn).ToArray());

        return new PricingAnalyticsSummaryDto
        {
            AveragePricePerSqm = metrics.Average,
            MedianPricePerSqm = metrics.Median,
            MinPricePerSqm = metrics.Min,
            MaxPricePerSqm = metrics.Max,
            MarketRangeLowPricePerSqm = metrics.Percentile25,
            MarketRangeHighPricePerSqm = metrics.Percentile75,
            ListingsCount = samples.Count,
            PeriodStart = period.Start,
            PeriodEnd = period.End,
            UpdatedAt = DateTimeOffset.UtcNow,
            ConfidenceLevel = GetConfidenceLevel(samples.Count),
            HasEnoughData = samples.Count >= MinimumGeneralSampleSize,
            TrendDirection = GetTrendDirection(samples)
        };
    }

    public async Task<IReadOnlyCollection<PricingAnalyticsPointDto>> GetTimeSeriesAsync(
        PricingAnalyticsQueryDto query,
        CancellationToken cancellationToken)
    {
        var samples = await LoadFilteredSamplesAsync(query, cancellationToken);

        return samples
            .Where(x => x.ReferenceDateUtc.HasValue)
            .GroupBy(x => new DateTime(x.ReferenceDateUtc!.Value.Year, x.ReferenceDateUtc.Value.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .OrderBy(x => x.Key)
            .Select(group =>
            {
                var metrics = BuildMetrics(group.Select(x => x.PricePerSqmByn).ToArray());
                return new PricingAnalyticsPointDto
                {
                    Period = $"{group.Key:yyyy-MM}",
                    AveragePricePerSqm = metrics.Average,
                    MedianPricePerSqm = metrics.Median,
                    ListingsCount = group.Count(),
                    HasEnoughData = group.Count() >= MinimumGeneralSampleSize,
                    ConfidenceLevel = GetConfidenceLevel(group.Count())
                };
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<PricingAnalyticsCategoryDto>> GetCategoriesAsync(
        PricingAnalyticsQueryDto query,
        CancellationToken cancellationToken)
    {
        var samples = await LoadFilteredSamplesAsync(query, cancellationToken);

        return samples
            .GroupBy(x => GetCategoryKey(x.BuildingType, x.ListingDetailsType))
            .Select(group =>
            {
                var metrics = BuildMetrics(group.Select(x => x.PricePerSqmByn).ToArray());
                return new PricingAnalyticsCategoryDto
                {
                    CategoryKey = group.Key,
                    CategoryLabel = GetCategoryLabel(group.Key),
                    AveragePricePerSqm = metrics.Average,
                    MedianPricePerSqm = metrics.Median,
                    MinPricePerSqm = metrics.Min,
                    MaxPricePerSqm = metrics.Max,
                    MarketRangeLowPricePerSqm = metrics.Percentile25,
                    MarketRangeHighPricePerSqm = metrics.Percentile75,
                    ListingsCount = group.Count(),
                    TrendDirection = GetTrendDirection(group.ToArray()),
                    ConfidenceLevel = GetConfidenceLevel(group.Count()),
                    HasEnoughData = group.Count() >= MinimumGeneralSampleSize
                };
            })
            .OrderByDescending(x => x.ListingsCount)
            .ToArray();
    }

    public async Task<ListingPriceComparisonDto?> GetListingComparisonAsync(Guid listingId, CancellationToken cancellationToken)
    {
        var listing = await context.ComparableListings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ListingId == listingId, cancellationToken);

        if (listing is null)
        {
            return null;
        }

        var currentPricePerSqm = TryGetPricePerSqmByn(listing);
        if (!currentPricePerSqm.HasValue)
        {
            return BuildComparisonResponse(
                currentPricePerSqm,
                localSamples: [],
                citySamples: [],
                countrySamples: []);
        }

        var localSamples = await LoadLocalComparableSamplesAsync(listing, cancellationToken);
        var cityName = InferCityName(listing.AddressText);
        var citySamples = string.IsNullOrWhiteSpace(cityName)
            ? []
            : await LoadFilteredSamplesAsync(new PricingAnalyticsQueryDto
            {
                ListingType = NormalizeListingType(listing.ListingType),
                Geography = "city",
                City = cityName,
                BuildingType = listing.BuildingType,
                ListingDetailsType = listing.ListingDetailsType,
                Months = 12
            }, cancellationToken);
        var countrySamples = await LoadFilteredSamplesAsync(new PricingAnalyticsQueryDto
        {
            ListingType = NormalizeListingType(listing.ListingType),
            Geography = "country",
            BuildingType = listing.BuildingType,
            ListingDetailsType = listing.ListingDetailsType,
            Months = 12
        }, cancellationToken);

        return BuildComparisonResponse(currentPricePerSqm, localSamples, citySamples, countrySamples);
    }

    public async Task<PricingEstimateResponseDto> GetEstimateAsync(
        PricingEstimateRequestDto request,
        CancellationToken cancellationToken)
    {
        var enteredPricePerSqm = request.AreaTotal > 0m
            ? decimal.Round(request.PriceAmountByn / request.AreaTotal, 2)
            : (decimal?)null;

        var localSamples = request.Latitude.HasValue && request.Longitude.HasValue
            ? await LoadLocalComparableSamplesAsync(
                NormalizeListingType(request.ListingType),
                request.BuildingType,
                request.ListingDetailsType,
                request.Latitude.Value,
                request.Longitude.Value,
                cancellationToken)
            : [];

        var citySamples = !string.IsNullOrWhiteSpace(request.CityName)
            ? await LoadFilteredSamplesAsync(new PricingAnalyticsQueryDto
            {
                ListingType = request.ListingType,
                Geography = "city",
                City = request.CityName,
                BuildingType = request.BuildingType,
                ListingDetailsType = request.ListingDetailsType,
                Months = 12
            }, cancellationToken)
            : [];

        var countrySamples = await LoadFilteredSamplesAsync(new PricingAnalyticsQueryDto
        {
            ListingType = request.ListingType,
            Geography = "country",
            BuildingType = request.BuildingType,
            ListingDetailsType = request.ListingDetailsType,
            Months = 12
        }, cancellationToken);

        var comparison = BuildComparisonResponse(enteredPricePerSqm, localSamples, citySamples, countrySamples);

        return new PricingEstimateResponseDto
        {
            EnteredPricePerSqm = enteredPricePerSqm,
            LocalRadiusAveragePricePerSqm = comparison.LocalRadiusAveragePricePerSqm,
            CityAveragePricePerSqm = comparison.CityAveragePricePerSqm,
            CountryAveragePricePerSqm = comparison.CountryAveragePricePerSqm,
            LocalRadiusListingsCount = comparison.LocalRadiusListingsCount,
            CityListingsCount = comparison.CityListingsCount,
            CountryListingsCount = comparison.CountryListingsCount,
            RelativePosition = comparison.RelativePosition,
            Message = comparison.Message,
            ConfidenceLevel = comparison.ConfidenceLevel,
            HasEnoughData = comparison.HasEnoughData
        };
    }

    private async Task<List<ComparableSample>> LoadFilteredSamplesAsync(
        PricingAnalyticsQueryDto query,
        CancellationToken cancellationToken)
    {
        var normalizedListingType = NormalizeListingType(query.ListingType);
        var baseQuery = context.ComparableListings
            .AsNoTracking()
            .Where(x =>
                x.AreaTotal > 0m &&
                (
                    (x.PriceAmountByn.HasValue && x.PriceAmountByn > 0m) ||
                    (x.PriceAmount.HasValue && x.PriceAmount > 0m && x.PriceCurrency == "BYN")
                ));

        var months = query.Months.GetValueOrDefault(DefaultMonths);
        if (months < 0)
        {
            months = DefaultMonths;
        }

        if (months > 0)
        {
            var minDate = DateTimeOffset.UtcNow.AddMonths(-months);
            baseQuery = baseQuery.Where(x => (x.PublishedDate ?? x.UpdatedAtUtc) >= minDate);
        }

        if (!string.IsNullOrWhiteSpace(normalizedListingType))
        {
            baseQuery = baseQuery.Where(x => x.ListingType == normalizedListingType);
        }

        if (!string.IsNullOrWhiteSpace(query.BuildingType))
        {
            baseQuery = baseQuery.Where(x => x.BuildingType == query.BuildingType);
        }

        if (!string.IsNullOrWhiteSpace(query.ListingDetailsType))
        {
            baseQuery = baseQuery.Where(x => x.ListingDetailsType == query.ListingDetailsType);
        }

        baseQuery = ApplyGeographyFilter(baseQuery, query.Geography, query.City);

        return await baseQuery
            .Select(x => new ComparableSample
            {
                ListingId = x.ListingId,
                AddressText = x.AddressText,
                BuildingType = x.BuildingType,
                ListingDetailsType = x.ListingDetailsType,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                ReferenceDateUtc = x.PublishedDate ?? x.UpdatedAtUtc,
                PricePerSqmByn = decimal.Round((x.PriceAmountByn ?? x.PriceAmount ?? 0m) / x.AreaTotal, 2)
            })
            .ToListAsync(cancellationToken);
    }

    private IQueryable<PricingComparableListing> ApplyGeographyFilter(
        IQueryable<PricingComparableListing> query,
        string? geography,
        string? city)
    {
        var normalized = geography?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "minsk" => query.Where(x => x.AddressText != null && EF.Functions.ILike(x.AddressText, "%минск%")),
            "city" when !string.IsNullOrWhiteSpace(city) => query.Where(x => x.AddressText != null && EF.Functions.ILike(x.AddressText, $"%{city.Trim()}%")),
            _ => query
        };
    }

    private async Task<List<ComparableSample>> LoadLocalComparableSamplesAsync(
        PricingComparableListing listing,
        CancellationToken cancellationToken)
    {
        if (!listing.Latitude.HasValue || !listing.Longitude.HasValue)
        {
            return [];
        }

        return await LoadLocalComparableSamplesAsync(
            NormalizeListingType(listing.ListingType),
            listing.BuildingType,
            listing.ListingDetailsType,
            listing.Latitude.Value,
            listing.Longitude.Value,
            cancellationToken,
            listing.ListingId);
    }

    private async Task<List<ComparableSample>> LoadLocalComparableSamplesAsync(
        string? listingType,
        string? buildingType,
        string? listingDetailsType,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken,
        Guid? excludedListingId = null)
    {
        var radiusMeters = 1500m;
        var bounds = BuildBounds(latitude, longitude, radiusMeters);

        var query = context.ComparableListings
            .AsNoTracking()
            .Where(x =>
                x.AreaTotal > 0m &&
                (
                    (x.PriceAmountByn.HasValue && x.PriceAmountByn > 0m) ||
                    (x.PriceAmount.HasValue && x.PriceAmount > 0m && x.PriceCurrency == "BYN")
                ) &&
                x.Latitude.HasValue &&
                x.Longitude.HasValue &&
                x.Latitude >= bounds.MinLatitude &&
                x.Latitude <= bounds.MaxLatitude &&
                x.Longitude >= bounds.MinLongitude &&
                x.Longitude <= bounds.MaxLongitude);

        if (!string.IsNullOrWhiteSpace(listingType))
        {
            query = query.Where(x => x.ListingType == listingType);
        }

        if (!string.IsNullOrWhiteSpace(buildingType))
        {
            query = query.Where(x => x.BuildingType == buildingType);
        }

        if (!string.IsNullOrWhiteSpace(listingDetailsType))
        {
            query = query.Where(x => x.ListingDetailsType == listingDetailsType);
        }

        if (excludedListingId.HasValue)
        {
            query = query.Where(x => x.ListingId != excludedListingId.Value);
        }

        var samples = await query
            .Select(x => new ComparableSample
            {
                ListingId = x.ListingId,
                AddressText = x.AddressText,
                BuildingType = x.BuildingType,
                ListingDetailsType = x.ListingDetailsType,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                ReferenceDateUtc = x.PublishedDate ?? x.UpdatedAtUtc,
                PricePerSqmByn = decimal.Round((x.PriceAmountByn ?? x.PriceAmount ?? 0m) / x.AreaTotal, 2)
            })
            .ToListAsync(cancellationToken);

        return samples
            .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
            .Where(x => CalculateDistanceMeters(latitude, longitude, x.Latitude!.Value, x.Longitude!.Value) <= radiusMeters)
            .ToList();
    }

    private static ListingPriceComparisonDto BuildComparisonResponse(
        decimal? currentPricePerSqm,
        IReadOnlyCollection<ComparableSample> localSamples,
        IReadOnlyCollection<ComparableSample> citySamples,
        IReadOnlyCollection<ComparableSample> countrySamples)
    {
        var localMetrics = localSamples.Count >= MinimumReliableLocalSampleSize
            ? BuildMetrics(localSamples.Select(x => x.PricePerSqmByn).ToArray())
            : SampleMetrics.Empty;
        var cityMetrics = citySamples.Count >= MinimumGeneralSampleSize
            ? BuildMetrics(citySamples.Select(x => x.PricePerSqmByn).ToArray())
            : SampleMetrics.Empty;
        var countryMetrics = countrySamples.Count >= MinimumGeneralSampleSize
            ? BuildMetrics(countrySamples.Select(x => x.PricePerSqmByn).ToArray())
            : SampleMetrics.Empty;

        var referenceAverage = localMetrics.Average ?? cityMetrics.Average ?? countryMetrics.Average;
        var relativePosition = DetermineRelativePosition(currentPricePerSqm, localMetrics.Average, cityMetrics.Average, countryMetrics.Average);
        var message = BuildRelativeMessage(relativePosition, localSamples.Count, citySamples.Count, countrySamples.Count);
        var confidenceCount = localMetrics.Average.HasValue ? localSamples.Count : cityMetrics.Average.HasValue ? citySamples.Count : countrySamples.Count;

        return new ListingPriceComparisonDto
        {
            CurrentListingPricePerSqm = currentPricePerSqm,
            LocalRadiusAveragePricePerSqm = localMetrics.Average,
            LocalRadiusMedianPricePerSqm = localMetrics.Median,
            LocalRadiusListingsCount = localSamples.Count,
            CityAveragePricePerSqm = cityMetrics.Average,
            CityMedianPricePerSqm = cityMetrics.Median,
            CityListingsCount = citySamples.Count,
            CountryAveragePricePerSqm = countryMetrics.Average,
            CountryMedianPricePerSqm = countryMetrics.Median,
            CountryListingsCount = countrySamples.Count,
            MarketRangeLowPricePerSqm = localMetrics.Percentile25 ?? cityMetrics.Percentile25 ?? countryMetrics.Percentile25,
            MarketRangeHighPricePerSqm = localMetrics.Percentile75 ?? cityMetrics.Percentile75 ?? countryMetrics.Percentile75,
            RelativePosition = relativePosition,
            Message = message,
            ConfidenceLevel = GetConfidenceLevel(confidenceCount),
            HasEnoughData = referenceAverage.HasValue
        };
    }

    private static string DetermineRelativePosition(
        decimal? current,
        decimal? localAverage,
        decimal? cityAverage,
        decimal? countryAverage)
    {
        if (!current.HasValue)
        {
            return "no_data";
        }

        var reference = localAverage ?? cityAverage ?? countryAverage;
        if (!reference.HasValue || reference.Value <= 0m)
        {
            return "no_data";
        }

        if (current.Value <= reference.Value * 0.9m)
        {
            return "below_average";
        }

        if (current.Value >= reference.Value * 1.1m)
        {
            return "above_average";
        }

        return "near_average";
    }

    private static string BuildRelativeMessage(
        string relativePosition,
        int localCount,
        int cityCount,
        int countryCount)
    {
        if (localCount < MinimumReliableLocalSampleSize && cityCount < MinimumGeneralSampleSize && countryCount < MinimumGeneralSampleSize)
        {
            return "Нет информации";
        }

        if (localCount < MinimumReliableLocalSampleSize)
        {
            return "Недостаточно данных рядом";
        }

        return relativePosition switch
        {
            "below_average" => "Цена ниже средней по району",
            "above_average" => "Цена выше средней по району",
            "near_average" => "Цена близка к средней по району",
            _ => "Нет информации"
        };
    }

    private static SampleMetrics BuildMetrics(IReadOnlyCollection<decimal> values)
    {
        if (values.Count == 0)
        {
            return SampleMetrics.Empty;
        }

        var sorted = values.OrderBy(x => x).ToArray();
        var average = decimal.Round(sorted.Average(), 2);

        return new SampleMetrics
        {
            Average = average,
            Median = Percentile(sorted, 0.5m),
            Min = sorted.First(),
            Max = sorted.Last(),
            Percentile25 = Percentile(sorted, 0.25m),
            Percentile75 = Percentile(sorted, 0.75m)
        };
    }

    private static decimal? Percentile(decimal[] sortedValues, decimal percentile)
    {
        if (sortedValues.Length == 0)
        {
            return null;
        }

        if (sortedValues.Length == 1)
        {
            return sortedValues[0];
        }

        var index = (sortedValues.Length - 1) * percentile;
        var lowerIndex = (int)Math.Floor(index);
        var upperIndex = (int)Math.Ceiling(index);

        if (lowerIndex == upperIndex)
        {
            return decimal.Round(sortedValues[lowerIndex], 2);
        }

        var fraction = index - lowerIndex;
        var lower = sortedValues[lowerIndex];
        var upper = sortedValues[upperIndex];
        return decimal.Round(lower + (upper - lower) * fraction, 2);
    }

    private static string GetConfidenceLevel(int count)
        => count switch
        {
            < 5 => "low",
            <= 20 => "medium",
            _ => "high"
        };

    private static string GetTrendDirection(IReadOnlyCollection<ComparableSample> samples)
    {
        var monthly = samples
            .Where(x => x.ReferenceDateUtc.HasValue)
            .GroupBy(x => new DateTime(x.ReferenceDateUtc!.Value.Year, x.ReferenceDateUtc.Value.Month, 1))
            .OrderByDescending(x => x.Key)
            .Take(4)
            .Select(x => x.Select(v => v.PricePerSqmByn).Average())
            .ToArray();

        if (monthly.Length < 2)
        {
            return "flat";
        }

        var current = monthly[0];
        var previousAverage = monthly.Skip(1).Average();
        if (current >= previousAverage * 1.03m)
        {
            return "up";
        }

        if (current <= previousAverage * 0.97m)
        {
            return "down";
        }

        return "flat";
    }

    private static (DateTimeOffset? Start, DateTimeOffset? End) ResolvePeriod(IReadOnlyCollection<ComparableSample> samples)
    {
        var dates = samples.Where(x => x.ReferenceDateUtc.HasValue).Select(x => x.ReferenceDateUtc!.Value).OrderBy(x => x).ToArray();
        return dates.Length == 0 ? (null, null) : (dates.First(), dates.Last());
    }

    private static string? InferCityName(string? addressText)
    {
        if (string.IsNullOrWhiteSpace(addressText))
        {
            return null;
        }

        if (addressText.Contains("минск", StringComparison.OrdinalIgnoreCase))
        {
            return "Минск";
        }

        var firstChunk = addressText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstChunk))
        {
            return null;
        }

        return firstChunk
            .Replace("г.", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("город", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string? NormalizeListingType(string? listingType)
        => listingType?.Trim().ToLowerInvariant() switch
        {
            "sale" => "Sale",
            "rent" => "Rent",
            "продажа" => "Sale",
            "аренда" => "Rent",
            _ => listingType
        };

    private static string GetCategoryKey(string? buildingType, string? listingDetailsType)
    {
        var details = listingDetailsType?.Trim().ToLowerInvariant();
        if (details == "apartment")
        {
            return "apartments";
        }

        if (details == "house" || details == "land")
        {
            return "houses";
        }

        if (details == "office")
        {
            return "offices";
        }

        if (details == "warehouse")
        {
            return "industrial";
        }

        var building = buildingType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (building.Contains("shopping") || building.Contains("mall"))
        {
            return "retail";
        }

        if (building.Contains("warehouse") || building.Contains("industrial"))
        {
            return "industrial";
        }

        if (building.Contains("business") || building.Contains("office"))
        {
            return "offices";
        }

        return "commercial";
    }

    private static string GetCategoryLabel(string categoryKey)
        => categoryKey switch
        {
            "apartments" => "Квартиры",
            "houses" => "Дома и участки",
            "offices" => "Офисы",
            "retail" => "Торговая недвижимость",
            "industrial" => "Склады и промышленная недвижимость",
            _ => "Коммерческая недвижимость"
        };

    private static decimal? TryGetPricePerSqmByn(PricingComparableListing listing)
    {
        if (listing.AreaTotal <= 0m)
        {
            return null;
        }

        var priceByn = listing.PriceAmountByn
            ?? (listing.PriceCurrency == "BYN" ? listing.PriceAmount : null);

        return priceByn.HasValue && priceByn > 0m
            ? decimal.Round(priceByn.Value / listing.AreaTotal, 2)
            : null;
    }

    private static RadiusBounds BuildBounds(decimal latitude, decimal longitude, decimal radiusMeters)
    {
        var latDelta = radiusMeters / 111_320m;
        var lonDelta = radiusMeters / (111_320m * (decimal)Math.Cos((double)latitude * Math.PI / 180d));

        return new RadiusBounds(
            latitude - latDelta,
            latitude + latDelta,
            longitude - lonDelta,
            longitude + lonDelta);
    }

    private static decimal CalculateDistanceMeters(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        const double earthRadiusMeters = 6371000d;

        var lat1 = (double)latitude1 * Math.PI / 180d;
        var lon1 = (double)longitude1 * Math.PI / 180d;
        var lat2 = (double)latitude2 * Math.PI / 180d;
        var lon2 = (double)longitude2 * Math.PI / 180d;

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var a = Math.Sin(deltaLat / 2d) * Math.Sin(deltaLat / 2d) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2d) * Math.Sin(deltaLon / 2d);

        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return decimal.Round((decimal)(earthRadiusMeters * c), 2);
    }

    private sealed class ComparableSample
    {
        public Guid ListingId { get; init; }
        public string? AddressText { get; init; }
        public string? BuildingType { get; init; }
        public string? ListingDetailsType { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public DateTimeOffset? ReferenceDateUtc { get; init; }
        public decimal PricePerSqmByn { get; init; }
    }

    private sealed class SampleMetrics
    {
        public static readonly SampleMetrics Empty = new();

        public decimal? Average { get; init; }
        public decimal? Median { get; init; }
        public decimal? Min { get; init; }
        public decimal? Max { get; init; }
        public decimal? Percentile25 { get; init; }
        public decimal? Percentile75 { get; init; }
    }

    private sealed record RadiusBounds(decimal MinLatitude, decimal MaxLatitude, decimal MinLongitude, decimal MaxLongitude);
}
