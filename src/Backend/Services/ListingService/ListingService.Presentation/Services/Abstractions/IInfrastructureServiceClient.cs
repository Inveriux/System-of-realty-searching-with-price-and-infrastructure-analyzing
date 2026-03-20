namespace ListingService.Presentation.Services.Abstractions;

public interface IInfrastructureServiceClient
{
    Task<ResolvedLocationDto?> ResolveLocationAsync(string addressText, CancellationToken cancellationToken);
    Task RefreshInfrastructureAsync(Guid locationId, CancellationToken cancellationToken);
}
