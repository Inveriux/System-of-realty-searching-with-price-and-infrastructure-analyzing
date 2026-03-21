namespace InfrastructureService.Services.Places.Abstractions;

public interface IPlacesProvider
{
    Task<IReadOnlyCollection<ExternalPoiResult>> GetNearbyPoisAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        IReadOnlyCollection<InfrastructureCategory> categories,
        int limit,
        CancellationToken cancellationToken);
}
