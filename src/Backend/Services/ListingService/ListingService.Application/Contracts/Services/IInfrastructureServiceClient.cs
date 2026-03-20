namespace ListingService.Application.Contracts.Services;

public interface IInfrastructureServiceClient
{
    Task<InfrastructureResolvedLocation?> ResolveLocationAsync(string addressText, CancellationToken cancellationToken);
    Task<bool> RefreshInfrastructureAsync(Guid locationId, CancellationToken cancellationToken);
}

public sealed record InfrastructureResolvedLocation(Guid LocationId, decimal Latitude, decimal Longitude);
