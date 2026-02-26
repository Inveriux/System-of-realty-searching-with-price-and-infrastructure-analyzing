using ListingService.Application.UseCases.Listings;
using BasicBuildingBlocks.Endpoints.Abstractions;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ListingService.Presentation.Endpoints.Listings;

public class ListingsEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("api/listings");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id}", GetByIdAsync);
        group.MapGet("/details/{id}", GetDetailedByIdAsync);
        group.MapPost("/", CreateListingAsync);

        return group;
    }

    public async Task<IResult> GetAllAsync(ISender sender, 
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 0, 
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetListingQuery(page, pageSize), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> GetByIdAsync(ISender sender, 
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetListingByIdQuery(id), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> GetDetailedByIdAsync(ISender sender, 
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDetailedListingByIdQuery(id), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateListingAsync(ISender sender, 
        [FromBody] CreateListingCommand listing,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(listing, cancellationToken);
        return result.ToHttpResult();
    }
}