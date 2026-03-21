using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients.Abstractions;

public interface IAgentServiceClient
{
    Task<DownstreamSellerPublicProfileDto?> GetPublicProfileAsync(Guid agentId, CancellationToken cancellationToken);
}
