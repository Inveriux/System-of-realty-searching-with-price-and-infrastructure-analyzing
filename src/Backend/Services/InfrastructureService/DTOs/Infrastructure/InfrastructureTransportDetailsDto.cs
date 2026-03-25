namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureTransportDetailsDto
{
    public decimal? NearestStopDistanceM { get; init; }
    public decimal? NearestStopWalkMinutes { get; init; }
    public int StopsCountPrimary { get; init; }
    public int StopGroupsCountPrimary { get; init; }
    public int StopPlatformsCountPrimary { get; init; }
    public decimal? NearestStopGroupDistanceM { get; init; }
    public decimal? NearestStopGroupWalkMinutes { get; init; }
    public decimal? NearestStopGroupDriveMinutes { get; init; }
    public bool NearestStopGroupDriveMinutesEstimated { get; init; }
    public string? NearestStopGroupName { get; init; }
    public int? NearestStopGroupPlatformCount { get; init; }
    public decimal? NearestStopGroupAnyDistanceM { get; init; }
    public decimal? NearestStopGroupAnyWalkMinutes { get; init; }
    public decimal? NearestStopGroupAnyDriveMinutes { get; init; }
    public bool NearestStopGroupAnyDriveMinutesEstimated { get; init; }
    public int? NearestStopGroupFoundRadiusM { get; init; }
    public IReadOnlyCollection<InfrastructureTransportStopGroupDto> StopGroups { get; init; } = [];
    public decimal? NearestMetroDistanceM { get; init; }
    public decimal? NearestMetroWalkMinutes { get; init; }
    public string? NearestMetroName { get; init; }
    public bool MetroFoundInPrimaryRadius { get; init; }
    public decimal? NearestParkingDistanceM { get; init; }
    public decimal? NearestParkingWalkMinutes { get; init; }
    public decimal? NearestParkingDriveMinutes { get; init; }
    public bool NearestParkingDriveMinutesEstimated { get; init; }
    public int ParkingCountPrimary { get; init; }
}
