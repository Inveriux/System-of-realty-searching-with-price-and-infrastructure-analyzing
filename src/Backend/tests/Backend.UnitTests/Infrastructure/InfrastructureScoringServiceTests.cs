using System.Text.Json;
using FluentAssertions;
using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Scoring;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Scoring.Policies;
using InfrastructureService.Services.TransportGrouping;
using Microsoft.Extensions.Options;
using Xunit;

namespace Backend.UnitTests.Infrastructure;

[Trait("Category", "Unit")]
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

    private readonly TransportStopGroupingService _transportStopGroupingService = new();

    [Fact]
    public void Calculate_WhenTransportContainsMetroAndGroupedStops_ShouldProduceHighTransportScore()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("transport", "bus_stop", "Площадь Победы", 90m, 53.9000m, 27.5600m),
            CreatePoi("transport", "tram_stop", "Площадь Победы", 95m, 53.9001m, 27.5601m),
            CreatePoi("transport", "bus_stop", "Парк Горького", 180m, 53.9010m, 27.5610m),
            CreatePoi("transport", "parking", "Парковка ТЦ", 160m, 53.9008m, 27.5590m),
            CreatePoi("transport", "metro_station", "Площадь Победы", 260m, 53.8996m, 27.5615m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        score.TransportScore.Should().BeGreaterThanOrEqualTo(70m);
        score.Details().CategoryScoreBreakdowns["transport"].DebugInfo["metro_applicable"].Should().Be("true");
    }

    [Fact]
    public void Calculate_WhenHealthcareContainsOnlyPharmacies_ShouldCapHealthcareScore()
    {
        var snapshot = CreateSnapshot(
        [
            CreatePoi("healthcare", "pharmacy", "Аптека 1", 60m, 53.9000m, 27.5600m),
            CreatePoi("healthcare", "pharmacy", "Аптека 2", 80m, 53.9002m, 27.5601m),
            CreatePoi("healthcare", "pharmacy", "Аптека 3", 120m, 53.9004m, 27.5602m)
        ]);

        var score = CreateScoringService().Calculate(MinskLocation, snapshot, BuildDetails(snapshot));

        score.HealthcareScore.Should().BeLessThanOrEqualTo(45m);
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
        => JsonSerializer.Deserialize<InfrastructureDetailsDto>(score.DetailedSummaryJson) ?? new InfrastructureDetailsDto();
}
