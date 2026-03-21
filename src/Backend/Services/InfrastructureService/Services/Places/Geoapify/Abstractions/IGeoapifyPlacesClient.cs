namespace InfrastructureService.Services.Places.Geoapify.Abstractions;

public interface IGeoapifyPlacesClient
{
    Task<IReadOnlyCollection<GeoapifyPlaceFeature>> GetPlacesAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        IReadOnlyCollection<string> categories,
        int limit,
        CancellationToken cancellationToken);
}
