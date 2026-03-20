namespace SearchService.Services.InternalClients.Abstractions;

public interface IInfrastructureServiceClient
{
    Task<Dictionary<string, decimal>> GetSearchFeaturesAsync(Guid locationId, CancellationToken cancellationToken);
}
