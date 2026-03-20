using InfrastructureService.Models;

namespace InfrastructureService.Data.Repositories.Abstractions;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Location?> GetByNormalizedAddressAsync(string normalizedAddress, CancellationToken cancellationToken);
    Task<Location?> GetByCoordinatesAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken);
    Task AddAsync(Location location, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
