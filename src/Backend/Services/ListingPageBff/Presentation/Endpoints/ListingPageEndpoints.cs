using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingPageBff.Application.ListingPages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ListingPageBff.Presentation.Endpoints;

public sealed class ListingPageEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/listing-pages")
            .WithTags("Listing Pages");

        group.MapGet("/{listingId:guid}", GetByIdAsync)
            .WithName("GetListingPage")
            .WithSummary("Aggregates the listing page DTO for the frontend.")
            .WithDescription("Loads listing details and enriches the page with infrastructure, seller and optional search-derived data.");

        return group;
    }

    public async Task<IResult> GetByIdAsync(
        ISender sender,
        [FromRoute] Guid listingId,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetListingPageQuery(listingId), cancellationToken)).ToHttpResult();
}
