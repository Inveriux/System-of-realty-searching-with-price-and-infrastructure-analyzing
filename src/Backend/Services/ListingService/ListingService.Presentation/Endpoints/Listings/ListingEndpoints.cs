using BasicBuildingBlocks.BusinessResults.Extensoins;
using ListingService.Application.UseCases.Listings;
using ListingService.Domain.Enums;
using ListingService.Presentation.Filters;
using BasicBuildingBlocks.Endpoints.Abstractions;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ListingService.Presentation.Endpoints.Listings;

public class ListingsEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("api/listings")
            .WithTags("Listings");

        group.MapGet("/", GetAllAsync)
            .WithName("GetListings")
            .WithSummary("Gets paged listings.")
            .WithDescription("Returns a paginated list of listings. Supports page and pageSize query parameters.");
        group.MapGet("/{id}", GetByIdAsync)
            .WithName("GetListingById")
            .WithSummary("Gets a listing by id.")
            .WithDescription("Returns the base listing projection without expanded listing details.");
        group.MapGet("/details/{id}", GetDetailedByIdAsync)
            .WithName("GetDetailedListingById")
            .WithSummary("Gets a listing with detailed payload.")
            .WithDescription("Returns the listing with media, price history and a polymorphic details object.");
        group.MapPost("/", CreateListingAsync)
            .RequireAuthorization()
            .WithName("CreateListing")
            .WithSummary("Creates a new listing.")
            .WithDescription("Accepts the common listing payload and a polymorphic listingDetails object selected by listingDetailsType.");
        group.MapDelete("/{id}", DeleteAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("DeleteListing")
            .WithSummary("Deletes a listing.")
            .WithDescription("Allows deletion only for the listing owner or an admin.");

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

    public async Task<IResult> DeleteAsync(ISender sender,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteListingCommand(id), cancellationToken);
        return result.ToHttpResult();
    }
}
