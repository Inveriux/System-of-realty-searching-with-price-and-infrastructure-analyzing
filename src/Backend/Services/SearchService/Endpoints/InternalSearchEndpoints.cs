using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.DTOs.Internal;
using SearchService.UseCases.Indexing;

namespace SearchService.Endpoints;

public sealed class InternalSearchEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/internal/search")
            .WithTags("Internal Search");

        group.MapPost("/index-listing", IndexListingAsync);
        group.MapPost("/reindex-location/{locationId:guid}", ReindexLocationAsync);
        group.MapDelete("/listings/{listingId:guid}", DeleteListingAsync);

        return group;
    }

    public async Task<IResult> IndexListingAsync(ISender sender, IndexListingRequestDto dto, CancellationToken cancellationToken)
        => (await sender.Send(new IndexListingCommand(dto), cancellationToken)).ToHttpResult();

    public async Task<IResult> ReindexLocationAsync(ISender sender, [FromRoute] Guid locationId, CancellationToken cancellationToken)
        => (await sender.Send(new ReindexLocationCommand(locationId), cancellationToken)).ToHttpResult();

    public async Task<IResult> DeleteListingAsync(ISender sender, [FromRoute] Guid listingId, CancellationToken cancellationToken)
        => (await sender.Send(new DeleteIndexedListingCommand(listingId), cancellationToken)).ToHttpResult();
}
