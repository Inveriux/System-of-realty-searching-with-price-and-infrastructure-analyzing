namespace InfrastructureService.Services.Places.Overpass.Abstractions;

public interface IOverpassApiClient
{
    Task<IReadOnlyCollection<OverpassElement>> QueryNearbyAsync(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        IReadOnlyCollection<OverpassTagFilter> filters,
        CancellationToken cancellationToken);
}
