using InfrastructureService.Models;
using InfrastructureService.Services.TransportGrouping;
using Xunit;

namespace InfrastructureService.Tests;

public sealed class TransportStopGroupingServiceTests
{
    private readonly TransportStopGroupingService _service = new();

    [Fact]
    public void SameNameWithin40Meters_ShouldCreateSingleGroup()
    {
        var groups = _service.Group(
        [
            CreateStop("Площадь Якуба Коласа", 53.9100m, 27.5800m, 15m),
            CreateStop("Площадь Якуба Коласа", 53.9102m, 27.5802m, 35m)
        ]);

        var group = Assert.Single(groups);
        Assert.Equal(2, group.PointCount);
    }

    [Fact]
    public void SameNameWithin130Meters_ShouldCreateSingleGroup()
    {
        var groups = _service.Group(
        [
            CreateStop("Площадь Якуба Коласа", 53.9100m, 27.5800m, 15m),
            CreateStop("Площадь Якуба Коласа", 53.9110m, 27.5805m, 120m)
        ]);

        Assert.Single(groups);
    }

    [Fact]
    public void SameNameWithin400Meters_ShouldCreateDifferentGroups()
    {
        var groups = _service.Group(
        [
            CreateStop("Площадь Якуба Коласа", 53.9100m, 27.5800m, 15m),
            CreateStop("Площадь Якуба Коласа", 53.9140m, 27.5800m, 400m)
        ]);

        Assert.Equal(2, groups.Count);
    }

    [Fact]
    public void SimilarAbbreviations_ShouldNormalizeIntoSingleGroup()
    {
        var groups = _service.Group(
        [
            CreateStop("Площадь Якуба Коласа", 53.9100m, 27.5800m, 15m),
            CreateStop("пл. Якуба Коласа", 53.9105m, 27.5801m, 60m)
        ]);

        var group = Assert.Single(groups);
        Assert.Equal("площадь якуба коласа", group.NormalizedName);
    }

    [Fact]
    public void MetroAndBusStopWithSimilarName_ShouldNotBecomeSingleGroup()
    {
        var groups = _service.Group(
        [
            CreateStop("Площадь Ленина", 53.8930m, 27.5470m, 20m, "bus_stop"),
            CreatePoi("Площадь Ленина", 53.8931m, 27.5471m, 30m, "metro_station")
        ]);

        var group = Assert.Single(groups);
        Assert.Equal("bus_stop", Assert.Single(group.StopTypes));
    }

    [Fact]
    public void DifferentNamesNearby_ShouldCreateDifferentGroups()
    {
        var groups = _service.Group(
        [
            CreateStop("Площадь Победы", 53.9090m, 27.5750m, 20m),
            CreateStop("Площадь Якуба Коласа", 53.9092m, 27.5752m, 30m)
        ]);

        Assert.Equal(2, groups.Count);
    }

    private static LocationPoiSnapshotItem CreateStop(string name, decimal latitude, decimal longitude, decimal distanceMeters, string subcategoryCode = "bus_stop")
        => CreatePoi(name, latitude, longitude, distanceMeters, subcategoryCode);

    private static LocationPoiSnapshotItem CreatePoi(string name, decimal latitude, decimal longitude, decimal distanceMeters, string subcategoryCode)
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
                    Name = "transport"
                }
            }
        };
}
