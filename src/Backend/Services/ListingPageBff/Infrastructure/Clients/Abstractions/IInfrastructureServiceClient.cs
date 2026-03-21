using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients.Abstractions;

public interface IInfrastructureServiceClient
{
    Task<DownstreamInfrastructureSummaryDto?> GetSummaryAsync(Guid locationId, CancellationToken cancellationToken);
    Task<DownstreamInfrastructureSummaryDto?> RefreshAsync(DownstreamRefreshInfrastructureRequestDto request, CancellationToken cancellationToken);
}
