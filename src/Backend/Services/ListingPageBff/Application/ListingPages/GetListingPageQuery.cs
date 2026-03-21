using BasicBuildingBlocks.BusinessResults;
using System.Text.Json;
using ListingPageBff.Contracts.ListingPages;
using ListingPageBff.Infrastructure.Clients.Abstractions;
using ListingPageBff.Infrastructure.Clients.Contracts;
using MediatR;

namespace ListingPageBff.Application.ListingPages;

public sealed record GetListingPageQuery(Guid ListingId) : IRequest<Result<ListingPageDto>>;

public sealed class GetListingPageHandler(
    IListingServiceClient listingServiceClient,
    IInfrastructureServiceClient infrastructureServiceClient,
    IAgentServiceClient agentServiceClient,
    ISearchServiceClient searchServiceClient,
    ListingPageAssembler assembler,
    ILogger<GetListingPageHandler> logger)
    : IRequestHandler<GetListingPageQuery, Result<ListingPageDto>>
{
    public async Task<Result<ListingPageDto>> Handle(GetListingPageQuery request, CancellationToken cancellationToken)
    {
        DownstreamDetailedListingDto? listing;

        try
        {
            listing = await listingServiceClient.GetDetailedListingAsync(request.ListingId, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(ex, "ListingService call failed for listing {ListingId}", request.ListingId);
            return Result.Fail<ListingPageDto>(
                ResultStatusCode.Fail,
                "Unable to load listing page. Listing service is unavailable.");
        }

        if (listing is null)
            return Result.NotFound<ListingPageDto>($"Listing '{request.ListingId}' was not found.");

        var warnings = new List<ListingPageWarningDto>();

        var infrastructureTask = TryGetInfrastructureAsync(listing, warnings, cancellationToken);
        var sellerTask = TryGetSellerAsync(listing.AgentId, warnings, cancellationToken);
        var searchTask = TryGetSearchAsync(listing.Id, warnings, cancellationToken);

        await Task.WhenAll(infrastructureTask, sellerTask, searchTask);

        var page = assembler.Build(
            listing,
            infrastructureTask.Result,
            sellerTask.Result,
            searchTask.Result,
            warnings);

        return Result.Ok(page);
    }

    private async Task<DownstreamInfrastructureSummaryDto?> TryGetInfrastructureAsync(
        DownstreamDetailedListingDto listing,
        ICollection<ListingPageWarningDto> warnings,
        CancellationToken cancellationToken)
    {
        try
        {
            DownstreamInfrastructureSummaryDto? infrastructure = null;

            if (listing.LocationId.HasValue)
                infrastructure = await infrastructureServiceClient.GetSummaryAsync(listing.LocationId.Value, cancellationToken);

            if (infrastructure is null || RequiresInfrastructureRefresh(infrastructure))
            {
                infrastructure = await TryRefreshInfrastructureAsync(listing, cancellationToken);
            }

            if (infrastructure is null)
            {
                warnings.Add(new ListingPageWarningDto
                {
                    Code = "infrastructure_not_found",
                    Source = "InfrastructureService",
                    Message = "Infrastructure summary was not found for the listing location."
                });
            }

            return infrastructure;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "InfrastructureService call failed for listing {ListingId}, location {LocationId}", listing.Id, listing.LocationId);
            warnings.Add(new ListingPageWarningDto
            {
                Code = "infrastructure_unavailable",
                Source = "InfrastructureService",
                Message = "Infrastructure block is temporarily unavailable."
            });
            return null;
        }
    }

    private async Task<DownstreamInfrastructureSummaryDto?> TryRefreshInfrastructureAsync(
        DownstreamDetailedListingDto listing,
        CancellationToken cancellationToken)
    {
        if (listing.LocationId is null && (!listing.Latitude.HasValue || !listing.Longitude.HasValue))
            return null;

        return await infrastructureServiceClient.RefreshAsync(new DownstreamRefreshInfrastructureRequestDto
        {
            LocationId = listing.LocationId,
            AddressText = listing.AddressText,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            SearchRadiusMeters = 1500
        }, cancellationToken);
    }

    private static bool RequiresInfrastructureRefresh(DownstreamInfrastructureSummaryDto infrastructure)
    {
        if (infrastructure.Score is null)
            return true;

        var hasEmptyScore = infrastructure.Score.TotalScore <= 0 && infrastructure.Score.PoiCount <= 0;
        var hasNoPois = infrastructure.Pois is null || infrastructure.Pois.Count == 0;
        return hasEmptyScore && hasNoPois;
    }

    private async Task<DownstreamSellerPublicProfileDto?> TryGetSellerAsync(
        Guid agentId,
        ICollection<ListingPageWarningDto> warnings,
        CancellationToken cancellationToken)
    {
        try
        {
            var seller = await agentServiceClient.GetPublicProfileAsync(agentId, cancellationToken);
            if (seller is null)
            {
                warnings.Add(new ListingPageWarningDto
                {
                    Code = "seller_not_found",
                    Source = "AgentService",
                    Message = "Seller profile is unavailable. Returning fallback seller block."
                });
            }

            return seller;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "AgentService call failed for agent {AgentId}", agentId);
            warnings.Add(new ListingPageWarningDto
            {
                Code = "seller_unavailable",
                Source = "AgentService",
                Message = "Seller profile service is temporarily unavailable. Returning fallback seller block."
            });
            return null;
        }
    }

    private async Task<DownstreamSearchListingDto?> TryGetSearchAsync(
        Guid listingId,
        ICollection<ListingPageWarningDto> warnings,
        CancellationToken cancellationToken)
    {
        try
        {
            return await searchServiceClient.GetListingAsync(listingId, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogInformation(ex, "SearchService enrichment skipped for listing {ListingId}", listingId);
            warnings.Add(new ListingPageWarningDto
            {
                Code = "search_enrichment_skipped",
                Source = "SearchService",
                Severity = "info",
                Message = "Search enrichment is unavailable. Listing page returned without optional search-derived data."
            });
            return null;
        }
    }
}
