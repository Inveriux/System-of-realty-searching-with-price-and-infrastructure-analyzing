namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureNearbyCategoryDto
{
    public string CategoryCode { get; init; } = string.Empty;
    public string SubcategoryCode { get; init; } = string.Empty;
    public int CountInPrimaryRadius { get; init; }
    public decimal? NearestDistanceM { get; init; }
    public decimal? NearestWalkMinutes { get; init; }
    public decimal? NearestDriveMinutes { get; init; }
    public bool NearestDriveMinutesEstimated { get; init; }
    public decimal? NearestAnyDistanceM { get; init; }
    public decimal? NearestAnyWalkMinutes { get; init; }
    public decimal? NearestAnyDriveMinutes { get; init; }
    public bool NearestAnyDriveMinutesEstimated { get; init; }
    public int? FoundInRadiusM { get; init; }
    public bool FoundInPrimaryRadius { get; init; }
}
