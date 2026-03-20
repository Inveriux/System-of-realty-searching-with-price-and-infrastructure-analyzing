using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.DTOs.Search;
using SearchService.UseCases.Search;

namespace SearchService.Endpoints;

public sealed class SearchEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/search")
            .WithTags("Search");

        group.MapPost("/listings", SearchListingsAsync);
        group.MapGet("/listings/{listingId:guid}", GetByIdAsync);

        return group;
    }

    public async Task<IResult> SearchListingsAsync(ISender sender, SearchListingsRequestDto dto, CancellationToken cancellationToken)
        => (await sender.Send(new SearchListingsQuery(dto), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetByIdAsync(ISender sender, [FromRoute] Guid listingId, CancellationToken cancellationToken)
        => (await sender.Send(new GetSearchListingByIdQuery(listingId), cancellationToken)).ToHttpResult();
}
