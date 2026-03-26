using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Scoring;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Scoring.Policies;
using InfrastructureService.Services.TransportGrouping;
using Microsoft.Extensions.Options;
using Xunit;

namespace InfrastructureService.Tests;

public sealed class InfrastructureScoringServiceTests
{
    private static readonly Location MinskLocation = new()
    {
        Id = Guid.NewGuid(),
        City = "Минск",
        AddressText = "Минск, Беларусь",
        Latitude = 53.9000m,
        Longitude = 27.5600m
    };

    private static readonly Location BrestLocation = new()
    {
        Id = Guid.NewGuid(),
        City = "Брест",
        AddressText = "Брест, Беларусь",
        Latitude = 52.0976m,
        Longitude = 23.7341m
    };

    private readonly TransportStopGroupingService _transportStopGroupingService = new();

    [Fact]
    public void Transport_ManyRailwayObjectsOnly_ShouldStayWellBelowMaximum()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "railway_station", "Минск-Пассажирский", 223m, 53.9000m, 27.5600m),
            CreatePoi("transport", "railway_station", "Минск-Пассажирский", 224m, 53.9001m, 27.5601m),
            CreatePoi("transport", "railway_station", "Минск-Пассажирский", 225m, 53.9002m, 27.5602m),
            CreatePoi("transport", "railway_station", "Минск-Пассажирский", 226m, 53.9003m, 27.5603m),
            CreatePoi("transport", "railway_station", "Минск-Пассажирский", 227m, 53.9004m, 27.5604m),
            CreatePoi("transport", "railway_station", "Минск-Пассажирский", 228m, 53.9005m, 27.5605m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.TransportScore <= 45m);
        Assert.Equal("missing_surface_transport", score.Details().CategoryScoreBreakdowns["transport"].AppliedCapReason);
    }

    [Fact]
    public void Transport_StopGroupsParkingMetro_ShouldProduceHighScore()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Площадь Победы", 90m, 53.9000m, 27.5600m),
            CreatePoi("transport", "tram_stop", "Площадь Победы", 95m, 53.9001m, 27.5601m),
            CreatePoi("transport", "bus_stop", "Парк Горького", 180m, 53.9010m, 27.5610m),
            CreatePoi("transport", "parking", "Парковка ТЦ", 160m, 53.9008m, 27.5590m),
            CreatePoi("transport", "parking", "Парковка БЦ", 240m, 53.9012m, 27.5588m),
            CreatePoi("transport", "metro_station", "Площадь Победы", 260m, 53.8996m, 27.5615m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.TransportScore >= 74m, $"Actual transport score: {score.TransportScore}");
        Assert.True(score.Details().CategoryScoreBreakdowns["transport"].CompletenessAchieved);
        Assert.Equal("true", score.Details().CategoryScoreBreakdowns["transport"].DebugInfo["metro_applicable"]);
    }

    [Fact]
    public void Transport_DuplicatedPlatformsSameStopName_ShouldBeGroupedWithoutInflation()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Универсам Рига", 210m, 53.9000m, 27.5600m),
            CreatePoi("transport", "bus_stop", "Универсам Рига", 215m, 53.9001m, 27.5601m),
            CreatePoi("transport", "tram_stop", "Универсам Рига", 220m, 53.9002m, 27.5602m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var transportBreakdown = score.Details().CategoryScoreBreakdowns["transport"];

        Assert.Equal(1, transportBreakdown.GroupedCounts["bus_stop_group"]);
        Assert.Equal(1, transportBreakdown.GroupedCounts["tram_stop_group"]);
        Assert.True(score.TransportScore <= 68m);
    }

    [Fact]
    public void Transport_OnlyParkingNearby_ShouldRemainLow()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "parking", "Парковка 1", 120m, 53.9000m, 27.5600m),
            CreatePoi("transport", "parking", "Парковка 2", 170m, 53.9007m, 27.5607m),
            CreatePoi("transport", "parking", "Парковка 3", 230m, 53.9014m, 27.5614m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.TransportScore <= 45m);
        Assert.Contains("surface_transport", score.Details().CategoryScoreBreakdowns["transport"].MissingImportantSubcategories);
    }

    [Fact]
    public void Transport_StopGroupsAndRailwayWithoutMetroParking_ShouldBeMiddleNotMaximum()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Площадь Победы", 95m, 53.9000m, 27.5600m),
            CreatePoi("transport", "tram_stop", "Площадь Победы", 105m, 53.9001m, 27.5601m),
            CreatePoi("transport", "bus_stop", "Парк Горького", 210m, 53.9010m, 27.5610m),
            CreatePoi("transport", "railway_station", "Минск-Пассажирский", 260m, 53.9018m, 27.5620m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.InRange(score.TransportScore, 45m, 80m);
        Assert.False(score.Details().CategoryScoreBreakdowns["transport"].CompletenessAchieved);
    }

    [Fact]
    public void Transport_NonMinskWithoutMetro_ShouldNotBePenalizedForMetroMissing()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Центр", 90m, 52.0976m, 23.7341m),
            CreatePoi("transport", "tram_stop", "Центр", 120m, 52.0978m, 23.7342m),
            CreatePoi("transport", "bus_stop", "Площадь", 180m, 52.0981m, 23.7350m),
            CreatePoi("transport", "parking", "Парковка", 220m, 52.0970m, 23.7337m)
        ]);

        var score = CreateScoringService().Calculate(BrestLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["transport"];

        Assert.Equal("false", breakdown.DebugInfo["metro_applicable"]);
        Assert.DoesNotContain("metro", breakdown.MissingImportantSubcategories);
        Assert.True(score.TransportScore >= 58m, $"Actual transport score: {score.TransportScore}");
    }

    [Fact]
    public void Transport_MinskWithoutMetro_ShouldStillMarkMetroAsRelevantGap()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Площадь Победы", 90m, 53.9000m, 27.5600m),
            CreatePoi("transport", "tram_stop", "Площадь Победы", 120m, 53.9002m, 27.5602m),
            CreatePoi("transport", "parking", "Парковка", 200m, 53.8995m, 27.5592m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["transport"];

        Assert.Equal("true", breakdown.DebugInfo["metro_applicable"]);
        Assert.Contains("metro", breakdown.MissingImportantSubcategories);
        Assert.True(score.TransportScore < 100m);
    }

    [Fact]
    public void Transport_ManySurfaceStopGroupsOnly_ShouldBeGoodButNotExcellent()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Группа 1", 70m, 53.9000m, 27.5600m),
            CreatePoi("transport", "bus_stop", "Группа 2", 120m, 53.9010m, 27.5610m),
            CreatePoi("transport", "tram_stop", "Группа 3", 180m, 53.9020m, 27.5620m),
            CreatePoi("transport", "trolleybus_stop", "Группа 4", 230m, 53.9030m, 27.5630m),
            CreatePoi("transport", "bus_stop", "Группа 5", 280m, 53.9040m, 27.5640m),
            CreatePoi("transport", "bus_stop", "Группа 6", 330m, 53.9050m, 27.5650m),
            CreatePoi("transport", "bus_stop", "Группа 7", 380m, 53.9060m, 27.5660m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["transport"];

        Assert.True(score.TransportScore >= 40m, $"Actual transport score: {score.TransportScore}");
        Assert.True(score.TransportScore <= 68m);
        Assert.Equal("surface_only", breakdown.AppliedCapReason);
        Assert.Equal("7", breakdown.DebugInfo["surface_stop_groups_count"]);
    }

    [Fact]
    public void Transport_FourQualitySurfaceStopGroups_ShouldBeUsefulButNotNearHundred()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Группа 1", 70m, 53.9000m, 27.5600m),
            CreatePoi("transport", "bus_stop", "Группа 2", 120m, 53.9010m, 27.5610m),
            CreatePoi("transport", "tram_stop", "Группа 3", 180m, 53.9020m, 27.5620m),
            CreatePoi("transport", "trolleybus_stop", "Группа 4", 230m, 53.9030m, 27.5630m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.InRange(score.TransportScore, 40m, 68m);
    }

    [Fact]
    public void Transport_SurfaceAndParkingOutsideMinsk_ShouldBeStrongWithoutMetroPenalty()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Центр", 80m, 52.0976m, 23.7341m),
            CreatePoi("transport", "tram_stop", "Центр", 115m, 52.0978m, 23.7342m),
            CreatePoi("transport", "bus_stop", "Вокзал", 170m, 52.0982m, 23.7350m),
            CreatePoi("transport", "parking", "Паркинг", 210m, 52.0970m, 23.7336m)
        ]);

        var score = CreateScoringService().Calculate(BrestLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.TransportScore >= 59m, $"Actual transport score: {score.TransportScore}");
    }

    [Fact]
    public void Healthcare_OnlyPharmacies_ShouldBeCapped()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("healthcare", "pharmacy", "Аптека 1", 60m, 53.9000m, 27.5600m),
            CreatePoi("healthcare", "pharmacy", "Аптека 2", 80m, 53.9002m, 27.5601m),
            CreatePoi("healthcare", "pharmacy", "Аптека 3", 120m, 53.9004m, 27.5602m),
            CreatePoi("healthcare", "pharmacy", "Аптека 4", 140m, 53.9006m, 27.5603m),
            CreatePoi("healthcare", "pharmacy", "Аптека 5", 160m, 53.9008m, 27.5604m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.HealthcareScore <= 45m);
    }

    [Fact]
    public void Healthcare_PharmacyClinicHospital_ShouldBeHigh()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("healthcare", "pharmacy", "Аптека", 80m, 53.9000m, 27.5600m),
            CreatePoi("healthcare", "clinic", "Поликлиника", 220m, 53.9008m, 27.5608m),
            CreatePoi("healthcare", "hospital", "Больница", 650m, 53.9030m, 27.5630m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.HealthcareScore >= 70m);
    }

    [Fact]
    public void Healthcare_ManyPharmaciesAndHospital_ShouldBeStrongerThanBefore()
    {
        var pharmacies = Enumerable.Range(1, 13)
            .Select(index => CreatePoi(
                "healthcare",
                "pharmacy",
                $"Аптека {index}",
                55m + (index * 18m),
                53.9000m + (index * 0.0001m),
                27.5600m + (index * 0.0001m)));

        var snapshot = CreateSnapshot(
        [
            ..pharmacies,
            CreatePoi("healthcare", "hospital", "Больница", 680m, 53.9050m, 27.5650m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["healthcare"];

        Assert.InRange(score.HealthcareScore, 55m, 75m);
        Assert.Equal(13, breakdown.GroupedCounts["pharmacy"]);
        Assert.True(breakdown.ScoreBySubcategory["hospital"] > 0m);
    }

    [Fact]
    public void Commerce_ManySupermarketsOnly_ShouldBeCapped()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("commerce", "supermarket", "Супермаркет 1", 70m, 53.9000m, 27.5600m),
            CreatePoi("commerce", "supermarket", "Супермаркет 2", 100m, 53.9002m, 27.5601m),
            CreatePoi("commerce", "supermarket", "Супермаркет 3", 130m, 53.9004m, 27.5602m),
            CreatePoi("commerce", "supermarket", "Супермаркет 4", 160m, 53.9006m, 27.5603m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.CommerceScore <= 45m);
    }

    [Fact]
    public void Commerce_SupermarketMallDrogerie_ShouldOutperformSingleSubtype()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("commerce", "supermarket", "Супермаркет", 90m, 53.9000m, 27.5600m),
            CreatePoi("commerce", "mall", "Dana Mall", 550m, 53.9020m, 27.5620m),
            CreatePoi("commerce", "drogerie", "Мила", 180m, 53.9008m, 27.5608m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.CommerceScore >= 50m);
    }

    [Fact]
    public void Education_OnlyUniversities_ShouldNotBeMaximum()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("education", "university", "БГУ", 200m, 53.9000m, 27.5600m),
            CreatePoi("education", "university", "БНТУ", 380m, 53.9017m, 27.5617m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.EducationScore <= 45m);
    }

    [Fact]
    public void Education_KindergartenAndSchoolNearby_ShouldBeHigh()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("education", "kindergarten", "Сад 1", 140m, 53.9000m, 27.5600m),
            CreatePoi("education", "school", "Школа 1", 180m, 53.9009m, 27.5608m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        Assert.True(score.EducationScore >= 65m);
    }

    [Fact]
    public void Leisure_FiveCafesAndCinema_ShouldBeSignificantlyHigherThanLegacyLowScore()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("leisure", "cafe", "Кафе 1", 45m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "cafe", "Кафе 2", 70m, 53.9001m, 27.5601m),
            CreatePoi("leisure", "cafe", "Кафе 3", 90m, 53.9002m, 27.5602m),
            CreatePoi("leisure", "cafe", "Кафе 4", 120m, 53.9003m, 27.5603m),
            CreatePoi("leisure", "cafe", "Кафе 5", 155m, 53.9004m, 27.5604m),
            CreatePoi("leisure", "cinema", "Кинотеатр", 240m, 53.9013m, 27.5611m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["leisure"];

        Assert.InRange(score.LeisureScore, 45m, 60m);
        Assert.True(breakdown.ScoreByBlock["daily_leisure"] >= 30m);
        Assert.True(breakdown.ScoreByBlock["culture"] >= 10m);
        Assert.Equal("45", breakdown.DebugInfo["DailyLeisureNearestDistance"]);
        Assert.Equal("2", breakdown.DebugInfo["PresentLeisureBlocks"].Split(',', StringSplitOptions.RemoveEmptyEntries).Length.ToString());
    }

    [Fact]
    public void Leisure_FiveCafesAndTheaterAtHalfKilometer_ShouldStillScoreWell()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("leisure", "cafe", "Кафе 1", 45m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "cafe", "Кафе 2", 70m, 53.9001m, 27.5601m),
            CreatePoi("leisure", "cafe", "Кафе 3", 90m, 53.9002m, 27.5602m),
            CreatePoi("leisure", "cafe", "Кафе 4", 120m, 53.9003m, 27.5603m),
            CreatePoi("leisure", "cafe", "Кафе 5", 155m, 53.9004m, 27.5604m),
            CreatePoi("leisure", "theater", "Театр", 500m, 53.9030m, 27.5630m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["leisure"];

        Assert.InRange(score.LeisureScore, 48m, 65m);
        Assert.True(breakdown.ScoreByBlock["culture"] >= 12m);
    }

    [Fact]
    public void Leisure_FiveCafesOnly_ShouldBeModerateButNotMax()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("leisure", "cafe", "Кафе 1", 45m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "cafe", "Кафе 2", 70m, 53.9001m, 27.5601m),
            CreatePoi("leisure", "cafe", "Кафе 3", 90m, 53.9002m, 27.5602m),
            CreatePoi("leisure", "cafe", "Кафе 4", 120m, 53.9003m, 27.5603m),
            CreatePoi("leisure", "cafe", "Кафе 5", 155m, 53.9004m, 27.5604m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["leisure"];

        Assert.InRange(score.LeisureScore, 30m, 45m);
        Assert.True(breakdown.ScoreByBlock["daily_leisure"] >= 30m);
        Assert.Equal(0m, breakdown.ScoreByBlock["culture"]);
        Assert.Equal("single_subcategory", breakdown.AppliedCapReason);
    }

    [Fact]
    public void Leisure_FourCafesRestaurantAndCinema_ShouldScoreAsGoodUrbanLeisure()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("leisure", "cafe", "Кафе 1", 45m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "cafe", "Кафе 2", 70m, 53.9001m, 27.5601m),
            CreatePoi("leisure", "cafe", "Кафе 3", 90m, 53.9002m, 27.5602m),
            CreatePoi("leisure", "cafe", "Кафе 4", 120m, 53.9003m, 27.5603m),
            CreatePoi("leisure", "restaurant", "Ресторан", 140m, 53.9005m, 27.5606m),
            CreatePoi("leisure", "cinema", "Кинотеатр", 260m, 53.9015m, 27.5611m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["leisure"];

        Assert.InRange(score.LeisureScore, 48m, 65m);
        Assert.True(breakdown.ScoreByBlock["daily_leisure"] >= 31m);
        Assert.True(breakdown.ScoreByBlock["culture"] >= 10m);
    }

    [Fact]
    public void Leisure_ParkFitnessCinema_ShouldBeHigh()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("leisure", "park", "Парк", 120m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "fitness", "Фитнес", 210m, 53.9009m, 27.5608m),
            CreatePoi("leisure", "cinema", "Кинотеатр", 380m, 53.9030m, 27.5630m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["leisure"];

        Assert.InRange(score.LeisureScore, 58m, 80m);
        Assert.Contains("green_recreation", breakdown.PresentBlocks);
        Assert.Contains("active_leisure", breakdown.PresentBlocks);
        Assert.Contains("culture", breakdown.PresentBlocks);
    }

    [Fact]
    public void Leisure_DuplicatedCafes_ShouldNotInflateScoreTooMuch()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("leisure", "cafe", "Surf Coffee", 80m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "cafe", "Surf Coffee", 82m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "cafe", "Surf Coffee", 84m, 53.9000m, 27.5600m),
            CreatePoi("leisure", "cafe", "Why Knot", 130m, 53.9008m, 27.5608m),
            CreatePoi("leisure", "cinema", "Кинотеатр", 260m, 53.9015m, 27.5611m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["leisure"];

        Assert.InRange(score.LeisureScore, 30m, 55m);
        Assert.Equal(2, breakdown.GroupedCounts["cafe"]);
        Assert.True(breakdown.ScoreByBlock["daily_leisure"] < 30m);
    }

    [Fact]
    public void Leisure_SingleDistantMuseum_ShouldRemainLowToModerate()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("leisure", "museum", "Музей", 2200m, 53.9200m, 27.5800m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));
        var breakdown = score.Details().CategoryScoreBreakdowns["leisure"];

        Assert.InRange(score.LeisureScore, 1m, 20m);
        Assert.True(breakdown.ScoreByBlock["culture"] > 0m);
        Assert.Equal("culture", Assert.Single(breakdown.PresentBlocks));
    }

    private InfrastructureDetailsDto BuildDetails(LocationPoiSnapshot snapshot)
    {
        var stopGroups = _transportStopGroupingService.Group(snapshot.Items.ToArray())
            .Select(static group => new InfrastructureTransportStopGroupDto
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
            })
            .ToArray();

        return new InfrastructureDetailsDto
        {
            TransportDetails = new InfrastructureTransportDetailsDto
            {
                StopGroups = stopGroups,
                StopPlatformsCountPrimary = snapshot.Items.Count(item => item.Poi?.Category?.Name == "transport" && item.Poi.SubcategoryCode?.Contains("stop", StringComparison.OrdinalIgnoreCase) == true),
                StopGroupsCountPrimary = stopGroups.Length
            }
        };
    }

    private static IInfrastructureScoringService CreateScoringService()
    {
        var options = Options.Create(new InfrastructureScoringOptions());
        var inputBuilder = new InfrastructureScoringInputBuilder();
        var metroApplicabilityService = new MetroApplicabilityService(options);
        IInfrastructureCategoryScoringPolicy[] policies =
        [
            new TransportCategoryScoringPolicy(metroApplicabilityService),
            new HealthcareCategoryScoringPolicy(),
            new CommerceCategoryScoringPolicy(),
            new EducationCategoryScoringPolicy(),
            new LeisureCategoryScoringPolicy()
        ];

        return new InfrastructureScoringService(options, inputBuilder, policies);
    }

    private static LocationPoiSnapshot CreateSnapshot(IEnumerable<LocationPoiSnapshotItem> items)
        => new()
        {
            Id = Guid.NewGuid(),
            LocationId = MinskLocation.Id,
            SearchRadiusMeters = 5000,
            Source = "test",
            RefreshedAtUtc = DateTime.UtcNow,
            Items = items.ToArray()
        };

    private static LocationPoiSnapshotItem CreatePoi(
        string categoryCode,
        string subcategoryCode,
        string name,
        decimal distanceMeters,
        decimal latitude,
        decimal longitude)
        => new()
        {
            Id = Guid.NewGuid(),
            PoiId = Guid.NewGuid(),
            DistanceMeters = distanceMeters,
            Poi = new Poi
            {
                Id = Guid.NewGuid(),
                Name = name,
                Latitude = latitude,
                Longitude = longitude,
                SubcategoryCode = subcategoryCode,
                Category = new PoiCategory
                {
                    Id = Guid.NewGuid(),
                    Name = categoryCode
                }
            }
        };
}

internal static class InfrastructureScoreTestExtensions
{
    public static InfrastructureDetailsDto Details(this InfrastructureScore score)
        => System.Text.Json.JsonSerializer.Deserialize<InfrastructureDetailsDto>(score.DetailedSummaryJson) ?? new InfrastructureDetailsDto();
}
