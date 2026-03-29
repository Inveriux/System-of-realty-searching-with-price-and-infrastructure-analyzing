using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using PricingAnalyticsService.DTOs;
using PricingAnalyticsService.Repositories.Abstractions;
using PricingAnalyticsService.Services;

namespace PricingAnalyticsService.Endpoints;

public sealed class PricingAnalyticsEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/pricing-analytics")
            .WithTags("Pricing analytics");

        group.MapGet("/summary", GetSummaryAsync);
        group.MapGet("/timeseries", GetTimeSeriesAsync);
        group.MapGet("/categories", GetCategoriesAsync);
        group.MapGet("/listings/{listingId:guid}/comparison", GetListingComparisonAsync);
        group.MapPost("/estimate", EstimateAsync);
        group.MapGet("/listings/{listingId:guid}/price-history", GetPriceHistoryAsync);
        group.MapPatch("/listings/{listingId:guid}/price", UpdatePriceAsync);
        group.MapPost("/listings/{listingId:guid}/price-history", AddPriceHistoryEntryAsync);
        group.MapPatch("/listings/{listingId:guid}/price-history/{historyEntryId:guid}", UpdatePriceHistoryEntryAsync);
        group.MapDelete("/listings/{listingId:guid}/price-history/{historyEntryId:guid}", DeletePriceHistoryEntryAsync);

        return group;
    }

    public async Task<Microsoft.AspNetCore.Http.IResult> GetSummaryAsync(
        IPricingAnalyticsRepository repository,
        [AsParameters] PricingAnalyticsQueryDto query,
        CancellationToken cancellationToken)
        => Result.Ok(await repository.GetSummaryAsync(query, cancellationToken)).ToHttpResult();

    public async Task<Microsoft.AspNetCore.Http.IResult> GetTimeSeriesAsync(
        IPricingAnalyticsRepository repository,
        [AsParameters] PricingAnalyticsQueryDto query,
        CancellationToken cancellationToken)
        => Result.Ok(await repository.GetTimeSeriesAsync(query, cancellationToken)).ToHttpResult();

    public async Task<Microsoft.AspNetCore.Http.IResult> GetCategoriesAsync(
        IPricingAnalyticsRepository repository,
        [AsParameters] PricingAnalyticsQueryDto query,
        CancellationToken cancellationToken)
        => Result.Ok(await repository.GetCategoriesAsync(query, cancellationToken)).ToHttpResult();

    public async Task<Microsoft.AspNetCore.Http.IResult> GetListingComparisonAsync(
        IPricingAnalyticsRepository repository,
        Guid listingId,
        CancellationToken cancellationToken)
    {
        var comparison = await repository.GetListingComparisonAsync(listingId, cancellationToken);
        return comparison is null
            ? Result.NotFound<ListingPriceComparisonDto>("Listing was not found.").ToHttpResult()
            : Result.Ok(comparison).ToHttpResult();
    }

    public async Task<Microsoft.AspNetCore.Http.IResult> EstimateAsync(
        IPricingAnalyticsRepository repository,
        PricingEstimateRequestDto request,
        CancellationToken cancellationToken)
        => Result.Ok(await repository.GetEstimateAsync(request, cancellationToken)).ToHttpResult();

    public async Task<Microsoft.AspNetCore.Http.IResult> GetPriceHistoryAsync(
        IListingPriceHistoryClient client,
        Guid listingId,
        CancellationToken cancellationToken)
    {
        var history = await client.GetPriceHistoryAsync(listingId, cancellationToken);
        return history is null
            ? Result.NotFound<ListingPriceHistoryDto>("Listing price history was not found.").ToHttpResult()
            : Result.Ok(history).ToHttpResult();
    }

    public async Task<Microsoft.AspNetCore.Http.IResult> UpdatePriceAsync(
        IListingPriceHistoryClient client,
        Guid listingId,
        UpdateListingPriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var history = await client.UpdatePriceAsync(listingId, request, cancellationToken);
        return history is null
            ? Result.Fail<ListingPriceHistoryDto>(ResultStatusCode.Fail, "Failed to update listing price history.").ToHttpResult()
            : Result.Ok(history).ToHttpResult();
    }

    public async Task<Microsoft.AspNetCore.Http.IResult> AddPriceHistoryEntryAsync(
        IListingPriceHistoryClient client,
        Guid listingId,
        CreateListingPriceHistoryEntryRequestDto request,
        CancellationToken cancellationToken)
    {
        var history = await client.AddPriceHistoryEntryAsync(listingId, request, cancellationToken);
        return history is null
            ? Result.Fail<ListingPriceHistoryDto>(ResultStatusCode.Fail, "Failed to add listing price history entry.").ToHttpResult()
            : Result.Ok(history).ToHttpResult();
    }

    public async Task<Microsoft.AspNetCore.Http.IResult> UpdatePriceHistoryEntryAsync(
        IListingPriceHistoryClient client,
        Guid listingId,
        Guid historyEntryId,
        UpdateListingPriceHistoryEntryRequestDto request,
        CancellationToken cancellationToken)
    {
        var history = await client.UpdatePriceHistoryEntryAsync(listingId, historyEntryId, request, cancellationToken);
        return history is null
            ? Result.Fail<ListingPriceHistoryDto>(ResultStatusCode.Fail, "Failed to update listing price history entry.").ToHttpResult()
            : Result.Ok(history).ToHttpResult();
    }

    public async Task<Microsoft.AspNetCore.Http.IResult> DeletePriceHistoryEntryAsync(
        IListingPriceHistoryClient client,
        Guid listingId,
        Guid historyEntryId,
        CancellationToken cancellationToken)
    {
        var history = await client.DeletePriceHistoryEntryAsync(listingId, historyEntryId, cancellationToken);
        return history is null
            ? Result.Fail<ListingPriceHistoryDto>(ResultStatusCode.Fail, "Failed to delete listing price history entry.").ToHttpResult()
            : Result.Ok(history).ToHttpResult();
    }
}
