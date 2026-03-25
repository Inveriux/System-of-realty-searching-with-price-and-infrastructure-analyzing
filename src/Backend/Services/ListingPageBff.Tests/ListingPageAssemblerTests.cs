using ListingPageBff.Application.ListingPages;
using ListingPageBff.Contracts.ListingPages;
using ListingPageBff.Infrastructure.Clients.Contracts;
using Xunit;

namespace ListingPageBff.Tests;

public sealed class ListingPageAssemblerTests
{
    private readonly ListingPageAssembler assembler = new(TimeProvider.System);

    [Fact]
    public void Build_KeepsAllRawMapPois()
    {
        var listing = CreateListing();
        var infrastructure = CreateInfrastructure();

        var result = assembler.Build(listing, infrastructure, null, null, []);

        Assert.NotNull(result.Infrastructure);
        Assert.Equal(3, result.Infrastructure!.RawMapPois.Count);
        Assert.Equal(result.Infrastructure.RawMapPois.Count, result.Infrastructure.NearbyPois.Count);
        Assert.Contains(result.Infrastructure.RawMapPois, poi => poi.Name == "Площадь Якуба Коласа");
        Assert.Contains(result.Infrastructure.RawMapPois, poi => poi.Name == "Академия наук");
        Assert.Contains(result.Infrastructure.RawMapPois, poi => poi.Name == "Парковка ТЦ");
    }

    [Fact]
    public void Build_MapsGroupedTransportAndCompactSummary()
    {
        var listing = CreateListing();
        var infrastructure = CreateInfrastructure();

        var result = assembler.Build(listing, infrastructure, null, null, []);
        var groupedTransport = result.Infrastructure!.GroupedTransportSummary;
        var stopItem = result.Infrastructure.CompactInfrastructureSummary.Single(item => item.Key == "stop_group");
        var metroItem = result.Infrastructure.CompactInfrastructureSummary.Single(item => item.Key == "metro");

        Assert.Equal("Площадь Якуба Коласа", groupedTransport.NearestStopGroupName);
        Assert.Equal(3, groupedTransport.StopGroupsCountPrimary);
        Assert.Equal(2, groupedTransport.NearestStopGroupPlatformCount);
        Assert.Equal(4.2m, groupedTransport.NearestMetroWalkMinutes);
        Assert.Equal("Академия наук", groupedTransport.NearestMetroName);
        Assert.Equal("Площадь Якуба Коласа", stopItem.EntityName);
        Assert.Equal("Платформ в ближайшей группе: 2", stopItem.Hint);
        Assert.True(metroItem.FoundInPrimaryRadius);
        Assert.Equal("Академия наук", metroItem.EntityName);
        Assert.Equal("/map-markers/minsk-metro.svg", metroItem.IconSrc);
    }

    [Fact]
    public void GroupPoisByCategory_ShouldDeduplicateLeisurePois()
    {
        var grouped = assembler.GroupPoisByCategory(
        [
            new DownstreamPoiDto
            {
                Id = Guid.NewGuid(),
                Name = "Парк Победы",
                Latitude = 53.91m,
                Longitude = 27.54m,
                Category = "leisure",
                SubcategoryCode = "park",
                DistanceMeters = 400m
            },
            new DownstreamPoiDto
            {
                Id = Guid.NewGuid(),
                Name = "Парк Победы",
                Latitude = 53.91001m,
                Longitude = 27.54001m,
                Category = "leisure",
                SubcategoryCode = "park",
                DistanceMeters = 402m
            }
        ]);

        var leisure = grouped.Single(group => group.Category == "Досуг");
        Assert.Single(leisure.Pois);
    }

    private static DownstreamDetailedListingDto CreateListing()
    {
        return new DownstreamDetailedListingDto
        {
            Id = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            LocationId = Guid.NewGuid(),
            Title = "Квартира у метро",
            AreaTotal = 54m,
            ListingType = "Sale"
        };
    }

    private static DownstreamInfrastructureSummaryDto CreateInfrastructure()
    {
        return new DownstreamInfrastructureSummaryDto
        {
            LocationId = Guid.NewGuid(),
            AddressText = "Минск",
            Latitude = 53.9m,
            Longitude = 27.56m,
            Score = new DownstreamInfrastructureScoreDto
            {
                TotalScore = 78m,
                TransportScore = 82m,
                PoiCount = 3,
                CalculatedAtUtc = DateTime.UtcNow
            },
            Pois =
            [
                new DownstreamPoiDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Площадь Якуба Коласа",
                    Latitude = 53.919m,
                    Longitude = 27.586m,
                    Category = "transport",
                    SubcategoryCode = "bus_stop",
                    DistanceMeters = 280m
                },
                new DownstreamPoiDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Академия наук",
                    Latitude = 53.921m,
                    Longitude = 27.6m,
                    Category = "transport",
                    SubcategoryCode = "metro_station",
                    DistanceMeters = 340m
                },
                new DownstreamPoiDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Парковка ТЦ",
                    Latitude = 53.918m,
                    Longitude = 27.584m,
                    Category = "transport",
                    SubcategoryCode = "parking",
                    DistanceMeters = 190m
                }
            ],
            NearbyCategories =
            [
                new DownstreamInfrastructureNearbyCategoryDto
                {
                    CategoryCode = "transport",
                    SubcategoryCode = "parking",
                    CountInPrimaryRadius = 1,
                    NearestDistanceM = 190m,
                    NearestWalkMinutes = 3m,
                    NearestDriveMinutes = 1m,
                    NearestDriveMinutesEstimated = true,
                    NearestAnyDistanceM = 190m,
                    NearestAnyWalkMinutes = 3m,
                    NearestAnyDriveMinutes = 1m,
                    NearestAnyDriveMinutesEstimated = true,
                    FoundInRadiusM = 1000,
                    FoundInPrimaryRadius = true
                }
            ],
            TransportDetails = new DownstreamInfrastructureTransportDetailsDto
            {
                NearestStopDistanceM = 280m,
                NearestStopWalkMinutes = 4m,
                StopsCountPrimary = 3,
                StopGroupsCountPrimary = 3,
                StopPlatformsCountPrimary = 6,
                NearestStopGroupDistanceM = 280m,
                NearestStopGroupWalkMinutes = 4m,
                NearestStopGroupDriveMinutes = 1m,
                NearestStopGroupDriveMinutesEstimated = true,
                NearestStopGroupName = "Площадь Якуба Коласа",
                NearestStopGroupPlatformCount = 2,
                NearestStopGroupAnyDistanceM = 280m,
                NearestStopGroupAnyWalkMinutes = 4m,
                NearestStopGroupAnyDriveMinutes = 1m,
                NearestStopGroupAnyDriveMinutesEstimated = true,
                NearestStopGroupFoundRadiusM = 800,
                StopGroups =
                [
                    new DownstreamInfrastructureTransportStopGroupDto
                    {
                        GroupId = "yakuba-kolasa",
                        NormalizedName = "площадь якуба коласа",
                        DisplayName = "Площадь Якуба Коласа",
                        CenterLatitude = 53.919m,
                        CenterLongitude = 27.586m,
                        MaxDistanceWithinGroup = 60m,
                        PointCount = 2,
                        PlatformCount = 2,
                        StopTypes = ["bus_stop"],
                        SourcePoiIds = [Guid.NewGuid()],
                        NearestDistanceMeters = 280m
                    }
                ],
                NearestMetroDistanceM = 340m,
                NearestMetroWalkMinutes = 4.2m,
                NearestMetroName = "Академия наук",
                MetroFoundInPrimaryRadius = true,
                NearestParkingDistanceM = 190m,
                NearestParkingWalkMinutes = 3m,
                NearestParkingDriveMinutes = 1m,
                NearestParkingDriveMinutesEstimated = true,
                ParkingCountPrimary = 1
            },
            SearchFeatures = new DownstreamInfrastructureSearchFeaturesDto
            {
                StopGroupsCountPrimary = 3,
                StopPlatformsCountPrimary = 6,
                NearestStopGroupWalkMinutes = 4m,
                NearestStopGroupDriveMinutes = 1m,
                NearestStopGroupDriveMinutesEstimated = true,
                NearestStopGroupName = "Площадь Якуба Коласа",
                NearestStopGroupPlatformCount = 2,
                NearestStopGroupAnyWalkMinutes = 4m,
                NearestStopGroupAnyDriveMinutes = 1m,
                NearestStopGroupAnyDriveMinutesEstimated = true,
                NearestParkingWalkMinutes = 3m,
                NearestParkingDriveMinutes = 1m,
                NearestParkingDriveMinutesEstimated = true,
                NearestParkingAnyWalkMinutes = 3m,
                NearestParkingAnyDriveMinutes = 1m,
                NearestParkingAnyDriveMinutesEstimated = true,
                NearestMetroWalkMinutes = 4.2m,
                NearestMetroName = "Академия наук",
                MetroFoundInPrimaryRadius = true
            }
        };
    }
}
