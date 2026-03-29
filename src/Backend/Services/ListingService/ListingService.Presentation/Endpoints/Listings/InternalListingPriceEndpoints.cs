using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Listings;
using ListingService.Application.UseCases.Listings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ListingService.Presentation.Endpoints.Listings;

public sealed class InternalListingPriceEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("internal/listings")
            .WithTags("InternalListingsPricing");

        group.MapGet("/{id:guid}/price-history", GetPriceHistoryAsync);
        group.MapPatch("/{id:guid}/price", UpdatePriceAsync);
        group.MapPost("/{id:guid}/price-history", AddPriceHistoryEntryAsync);
        group.MapPatch("/{id:guid}/price-history/{historyEntryId:guid}", UpdatePriceHistoryEntryAsync);
        group.MapDelete("/{id:guid}/price-history/{historyEntryId:guid}", DeletePriceHistoryEntryAsync);

        return group;
    }

    public async Task<IResult> GetPriceHistoryAsync(
        ISender sender,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetListingPriceHistoryQuery(id), cancellationToken)).ToHttpResult();

    public async Task<IResult> UpdatePriceAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromBody] UpdateListingPriceRequestDto request,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateListingPriceCommand(id, request), cancellationToken)).ToHttpResult();

    public async Task<IResult> AddPriceHistoryEntryAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromBody] CreateListingPriceHistoryEntryRequestDto request,
        CancellationToken cancellationToken)
        => (await sender.Send(new AddListingPriceHistoryEntryCommand(id, request), cancellationToken)).ToHttpResult();

    public async Task<IResult> UpdatePriceHistoryEntryAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromRoute] Guid historyEntryId,
        [FromBody] UpdateListingPriceHistoryEntryRequestDto request,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateListingPriceHistoryEntryCommand(id, historyEntryId, request), cancellationToken)).ToHttpResult();

    public async Task<IResult> DeletePriceHistoryEntryAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromRoute] Guid historyEntryId,
        CancellationToken cancellationToken)
        => (await sender.Send(new DeleteListingPriceHistoryEntryCommand(id, historyEntryId), cancellationToken)).ToHttpResult();
}
