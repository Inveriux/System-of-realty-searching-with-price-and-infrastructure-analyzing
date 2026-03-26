using SearchService.DTOs.Search;
using SearchService.Models;

namespace SearchService.Services.Ranking;

public sealed class SearchRankingService : ISearchRankingService
{
    public SearchRankingResult Rank(SearchListing listing, SearchListingsRequestDto request, SearchRankingWeightsDto weights)
    {
        var baseMatchScore = ComputeBaseMatchScore(listing, request);
        var priceFitScore = ComputeBandFitScore(
            request.PriceMin ?? request.MinPrice,
            request.PriceMax ?? request.MaxPrice,
            listing.PriceAmountByn ?? listing.PriceAmountUsd ?? listing.PriceAmount,
            maxScore: 16m,
            weight: weights.PriceWeight);
        var areaFitScore = ComputeBandFitScore(
            request.AreaMin ?? request.MinArea,
            request.AreaMax ?? request.MaxArea,
            listing.AreaTotal,
            maxScore: 10m,
            weight: weights.AreaWeight);
        var locationFitScore = ComputeLocationFitScore(listing, request, weights.LocationWeight);
        var infrastructureFitScore = ComputeInfrastructureFitScore(listing, request, weights);
        var preferenceAlignmentScore = ComputePreferenceAlignmentScore(listing, weights);
        var freshnessScore = ComputeFreshnessScore(listing.PublishedDate);

        var finalScore = baseMatchScore +
                         priceFitScore +
                         areaFitScore +
                         locationFitScore +
                         infrastructureFitScore +
                         preferenceAlignmentScore +
                         freshnessScore;

        return new SearchRankingResult
        {
            FinalScore = decimal.Round(finalScore, 2, MidpointRounding.AwayFromZero),
            Breakdown = new SearchRelevanceBreakdownDto
            {
                BaseMatchScore = decimal.Round(baseMatchScore, 2, MidpointRounding.AwayFromZero),
                PriceFitScore = decimal.Round(priceFitScore, 2, MidpointRounding.AwayFromZero),
                AreaFitScore = decimal.Round(areaFitScore, 2, MidpointRounding.AwayFromZero),
                LocationFitScore = decimal.Round(locationFitScore, 2, MidpointRounding.AwayFromZero),
                InfrastructureFitScore = decimal.Round(infrastructureFitScore, 2, MidpointRounding.AwayFromZero),
                PreferenceAlignmentScore = decimal.Round(preferenceAlignmentScore, 2, MidpointRounding.AwayFromZero),
                FreshnessScore = decimal.Round(freshnessScore, 2, MidpointRounding.AwayFromZero),
                FinalScore = decimal.Round(finalScore, 2, MidpointRounding.AwayFromZero)
            },
            Highlights = BuildHighlights(listing, request)
        };
    }

    private static decimal ComputeBaseMatchScore(SearchListing listing, SearchListingsRequestDto request)
    {
        var score = 6m;
        var operationType = request.OperationType ?? request.ListingType;
        var propertyType = request.PropertyType ?? request.BuildingType;

        if (!string.IsNullOrWhiteSpace(operationType) &&
            string.Equals(listing.ListingType, operationType, StringComparison.OrdinalIgnoreCase))
        {
            score += 3m;
        }

        if (!string.IsNullOrWhiteSpace(propertyType) &&
            string.Equals(listing.BuildingType, propertyType, StringComparison.OrdinalIgnoreCase))
        {
            score += 3m;
        }

        if (string.IsNullOrWhiteSpace(request.Query))
            return score;

        var query = request.Query.Trim();
        if (listing.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            score += 8m;
        else if ((listing.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (listing.AddressText?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            score += 4m;

        return score;
    }

    private static decimal ComputeBandFitScore(decimal? min, decimal? max, decimal? value, decimal maxScore, decimal weight)
    {
        if (!value.HasValue)
            return 0m;

        var multiplier = Clamp(weight, 0.4m, 1.8m);

        if (!min.HasValue && !max.HasValue)
            return maxScore * 0.35m * multiplier;

        if (min.HasValue && max.HasValue)
        {
            var range = Math.Max(1m, max.Value - min.Value);
            var center = min.Value + range / 2m;
            var distance = Math.Abs(value.Value - center);
            var normalized = 1m - Math.Min(1m, distance / (range / 2m + 1m));
            return maxScore * (0.45m + 0.55m * normalized) * multiplier;
        }

        if (min.HasValue)
        {
            var delta = Math.Max(0m, value.Value - min.Value);
            var normalized = 1m / (1m + delta / Math.Max(1m, min.Value * 0.25m));
            return maxScore * (0.45m + 0.55m * normalized) * multiplier;
        }

        var below = Math.Max(0m, max!.Value - value.Value);
        var belowNormalized = 1m / (1m + below / Math.Max(1m, max.Value * 0.25m));
        return maxScore * (0.45m + 0.55m * belowNormalized) * multiplier;
    }

    private static decimal ComputeLocationFitScore(SearchListing listing, SearchListingsRequestDto request, decimal locationWeight)
    {
        var multiplier = Clamp(locationWeight, 0.4m, 1.8m);
        var centerLatitude = request.CenterLatitude ?? request.Latitude;
        var centerLongitude = request.CenterLongitude ?? request.Longitude;

        if (centerLatitude.HasValue && centerLongitude.HasValue && request.RadiusMeters.HasValue &&
            listing.Latitude.HasValue && listing.Longitude.HasValue)
        {
            var distanceMeters = GetDistanceMeters(
                centerLatitude.Value,
                centerLongitude.Value,
                listing.Latitude.Value,
                listing.Longitude.Value);
            var normalized = 1m - Math.Min(1m, distanceMeters / Math.Max(1m, request.RadiusMeters.Value));
            return (4m + 11m * normalized) * multiplier;
        }

        if (request.BoundsMinLatitude.HasValue && request.BoundsMinLongitude.HasValue &&
            request.BoundsMaxLatitude.HasValue && request.BoundsMaxLongitude.HasValue &&
            listing.Latitude.HasValue && listing.Longitude.HasValue)
        {
            var centerLat = (request.BoundsMinLatitude.Value + request.BoundsMaxLatitude.Value) / 2m;
            var centerLon = (request.BoundsMinLongitude.Value + request.BoundsMaxLongitude.Value) / 2m;
            var maxDistance = GetDistanceMeters(
                request.BoundsMinLatitude.Value,
                request.BoundsMinLongitude.Value,
                request.BoundsMaxLatitude.Value,
                request.BoundsMaxLongitude.Value) / 2m;
            var distance = GetDistanceMeters(centerLat, centerLon, listing.Latitude.Value, listing.Longitude.Value);
            var normalized = 1m - Math.Min(1m, distance / Math.Max(1m, maxDistance));
            return (4m + 9m * normalized) * multiplier;
        }

        if (request.LocationId.HasValue && listing.LocationId == request.LocationId)
            return 10m * multiplier;

        return 3m * multiplier;
    }

    private static decimal ComputeInfrastructureFitScore(SearchListing listing, SearchListingsRequestDto request, SearchRankingWeightsDto weights)
    {
        var weightedCategories = new[]
        {
            ((listing.TransportScore ?? 0m) / 100m, weights.TransportWeight),
            ((listing.EducationScore ?? 0m) / 100m, weights.EducationWeight),
            ((listing.HealthcareScore ?? 0m) / 100m, weights.HealthcareWeight),
            ((listing.CommerceScore ?? 0m) / 100m, weights.CommerceWeight),
            ((listing.LeisureScore ?? 0m) / 100m, weights.LeisureWeight)
        };

        var categoryWeightSum = weightedCategories.Sum(x => x.Item2);
        var categoryFit = categoryWeightSum <= 0m
            ? 0m
            : weightedCategories.Sum(x => x.Item1 * x.Item2) / categoryWeightSum;

        var score = 18m * categoryFit;

        score += ComputePracticalFit(request.MaxStopWalkMinutes, listing.NearestStopWalkMinutes, 5m);
        score += ComputePracticalFit(request.MaxStoreWalkMinutes, listing.NearestStoreWalkMinutes, 4m);
        score += ComputePracticalFit(request.MaxPharmacyWalkMinutes, listing.NearestPharmacyWalkMinutes, 4m);
        score += ComputePracticalFit(request.MaxSchoolWalkMinutes, listing.NearestSchoolWalkMinutes, 4m);
        score += ComputePracticalFit(request.MaxKindergartenWalkMinutes, listing.NearestKindergartenWalkMinutes, 4m);
        score += ComputePracticalFit(request.MaxParkWalkMinutes, listing.NearestParkWalkMinutes, 4m);
        score += ComputePracticalFit(request.MaxFitnessWalkMinutes, listing.NearestFitnessWalkMinutes, 3m);
        score += ComputePracticalFit(request.MaxMetroWalkMinutes, listing.NearestMetroWalkMinutes, 4m);
        score += ComputePracticalFit(request.MaxClinicDriveMinutes, listing.NearestClinicDriveMinutes, 3m);
        score += ComputePracticalFit(request.MaxHospitalDriveMinutes, listing.NearestHospitalDriveMinutes, 3m);
        score += ComputePracticalFit(request.MaxCinemaDriveMinutes, listing.NearestCinemaDriveMinutes, 3m);
        score += ComputePracticalFit(request.MaxTheaterDriveMinutes, listing.NearestTheaterDriveMinutes, 2m);
        score += ComputePracticalFit(request.MaxMuseumDriveMinutes, listing.NearestMuseumDriveMinutes, 2m);

        score += ComputeSoftMetricBonus(listing.NearestStopWalkMinutes, 7m, 3m, weights.TransportWeight);
        score += ComputeSoftMetricBonus(listing.NearestStoreWalkMinutes, 10m, 2m, weights.CommerceWeight);
        score += ComputeSoftMetricBonus(listing.NearestPharmacyWalkMinutes, 12m, 2m, weights.HealthcareWeight);
        score += ComputeSoftMetricBonus(listing.NearestSchoolWalkMinutes, 15m, 2m, weights.EducationWeight);
        score += ComputeSoftMetricBonus(listing.NearestKindergartenWalkMinutes, 15m, 2m, weights.EducationWeight);
        score += ComputeSoftMetricBonus(listing.NearestParkWalkMinutes, 15m, 2m, weights.LeisureWeight);
        score += ComputeSoftMetricBonus(listing.NearestFitnessWalkMinutes, 15m, 2m, weights.LeisureWeight);
        score += ComputeSoftMetricBonus(listing.NearestMetroWalkMinutes, 14m, 2m, weights.TransportWeight);

        return Math.Min(32m, score);
    }

    private static decimal ComputePreferenceAlignmentScore(SearchListing listing, SearchRankingWeightsDto weights)
    {
        var score = 0m;
        score += ((listing.TransportScore ?? 0m) / 100m) * 2.5m * Clamp(weights.TransportWeight, 0.5m, 1.8m);
        score += ((listing.EducationScore ?? 0m) / 100m) * 2.5m * Clamp(weights.EducationWeight, 0.5m, 1.8m);
        score += ((listing.HealthcareScore ?? 0m) / 100m) * 2m * Clamp(weights.HealthcareWeight, 0.5m, 1.8m);
        score += ((listing.CommerceScore ?? 0m) / 100m) * 2m * Clamp(weights.CommerceWeight, 0.5m, 1.8m);
        score += ((listing.LeisureScore ?? 0m) / 100m) * 3m * Clamp(weights.LeisureWeight, 0.5m, 1.8m);
        return Math.Min(12m, score);
    }

    private static decimal ComputeFreshnessScore(DateTimeOffset? publishedDate)
    {
        if (!publishedDate.HasValue)
            return 0m;

        var ageDays = Math.Max(0d, (DateTimeOffset.UtcNow - publishedDate.Value).TotalDays);
        var freshness = Math.Max(0d, 1d - ageDays / 90d);
        return (decimal)freshness * 5m;
    }

    private static decimal ComputePracticalFit(decimal? requestedMax, decimal? actualValue, decimal maxScore)
    {
        if (!requestedMax.HasValue || !actualValue.HasValue)
            return 0m;

        var ratio = actualValue.Value / Math.Max(1m, requestedMax.Value);
        var normalized = ratio switch
        {
            <= 0.5m => 1m,
            <= 1m => 1m - (ratio - 0.5m) * 0.4m,
            _ => 0m
        };

        return maxScore * normalized;
    }

    private static decimal ComputeSoftMetricBonus(decimal? actualValue, decimal targetMinutes, decimal maxScore, decimal weight)
    {
        if (!actualValue.HasValue)
            return 0m;

        var ratio = actualValue.Value / Math.Max(1m, targetMinutes);
        var normalized = Math.Max(0m, 1m - ratio / 2m);
        return maxScore * normalized * Clamp(weight, 0.5m, 1.5m);
    }

    private static IReadOnlyCollection<string> BuildHighlights(SearchListing listing, SearchListingsRequestDto request)
    {
        var highlights = new List<(decimal value, string text)>();

        AddHighlight(highlights, listing.NearestStopWalkMinutes, "Остановка");
        AddHighlight(highlights, listing.NearestStoreWalkMinutes, "Магазин");
        AddHighlight(highlights, listing.NearestPharmacyWalkMinutes, "Аптека");
        AddHighlight(highlights, listing.NearestSchoolWalkMinutes, "Школа");
        AddHighlight(highlights, listing.NearestKindergartenWalkMinutes, "Детский сад");
        AddHighlight(highlights, listing.NearestParkWalkMinutes, "Парк");
        AddHighlight(highlights, listing.NearestFitnessWalkMinutes, "Фитнес");
        AddHighlight(highlights, listing.NearestMetroWalkMinutes, "Метро");

        if (request.MaxClinicDriveMinutes.HasValue && listing.NearestClinicDriveMinutes.HasValue)
            highlights.Add((listing.NearestClinicDriveMinutes.Value, $"Поликлиника — {FormatMinutes(listing.NearestClinicDriveMinutes.Value)} на машине"));

        if (request.MaxHospitalDriveMinutes.HasValue && listing.NearestHospitalDriveMinutes.HasValue)
            highlights.Add((listing.NearestHospitalDriveMinutes.Value, $"Больница — {FormatMinutes(listing.NearestHospitalDriveMinutes.Value)} на машине"));

        return highlights
            .OrderBy(x => x.value)
            .Select(x => x.text)
            .Distinct(StringComparer.Ordinal)
            .Take(5)
            .ToArray();
    }

    private static void AddHighlight(List<(decimal value, string text)> highlights, decimal? minutes, string label)
    {
        if (!minutes.HasValue)
            return;

        highlights.Add((minutes.Value, $"{label} — {FormatMinutes(minutes.Value)} пешком"));
    }

    private static string FormatMinutes(decimal value)
        => $"{decimal.Round(value, 0, MidpointRounding.AwayFromZero)} мин";

    private static decimal GetDistanceMeters(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        const double earthRadiusMeters = 6_371_000d;
        var lat1 = DegreesToRadians((double)latitude1);
        var lon1 = DegreesToRadians((double)longitude1);
        var lat2 = DegreesToRadians((double)latitude2);
        var lon2 = DegreesToRadians((double)longitude2);
        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;
        var a = Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return (decimal)(earthRadiusMeters * c);
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180d;

    private static decimal Clamp(decimal value, decimal min, decimal max)
        => Math.Max(min, Math.Min(max, value));
}
