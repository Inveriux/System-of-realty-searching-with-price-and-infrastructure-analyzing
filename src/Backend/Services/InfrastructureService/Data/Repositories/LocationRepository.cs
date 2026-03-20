using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Repositories;

public sealed class LocationRepository(ApplicationDbContext context) : ILocationRepository
{
    public Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => context.Locations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Location?> GetByNormalizedAddressAsync(string normalizedAddress, CancellationToken cancellationToken)
        => context.Locations.FirstOrDefaultAsync(x => x.NormalizedAddressText == normalizedAddress, cancellationToken);

    public Task<Location?> GetByCoordinatesAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken)
        => context.Locations.FirstOrDefaultAsync(x => x.Latitude == latitude && x.Longitude == longitude, cancellationToken);

    public Task AddAsync(Location location, CancellationToken cancellationToken)
        => context.Locations.AddAsync(location, cancellationToken).AsTask();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
