using ListingService.Application.Contracts.Repositories;
using ListingService.Application.DTOs.Listings;
using MediatR;

namespace ListingService.Application.UseCases.Listings;

public sealed record GetListingPriceHistoryQuery(Guid ListingId) : IRequest<Result<ListingPriceHistoryTimelineDto>>;

public sealed class GetListingPriceHistoryHandler(IRepositoryManager manager)
    : IRequestHandler<GetListingPriceHistoryQuery, Result<ListingPriceHistoryTimelineDto>>
{
    public async Task<Result<ListingPriceHistoryTimelineDto>> Handle(
        GetListingPriceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var listing = await manager.Listing.GetDetailedByIdAsync(request.ListingId, false, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound<ListingPriceHistoryTimelineDto>("Объявление не найдено.");
        }

        var history = await manager.Listing.GetPriceHistoryAsync(request.ListingId, cancellationToken);
        var orderedHistory = history.OrderBy(x => x.ChangedAt).ThenBy(x => x.Id).ToArray();
        var first = orderedHistory.FirstOrDefault();
        var last = orderedHistory.LastOrDefault();

        return Result.Ok(new ListingPriceHistoryTimelineDto
        {
            ListingId = request.ListingId,
            CurrentPriceAmountByn = listing.PriceAmountByn,
            FirstRecordedPriceAmountByn = first?.PriceAmountByn,
            LastRecordedPriceAmountByn = last?.PriceAmountByn,
            FirstRecordedAt = first?.ChangedAt,
            LastRecordedAt = last?.ChangedAt,
            PercentChangeFromFirst = CalculatePercentChange(first?.PriceAmountByn, last?.PriceAmountByn),
            History = orderedHistory.Select(entry => new ListingPriceHistoryTimelinePointDto
            {
                Id = entry.Id,
                EffectiveDateUtc = entry.ChangedAt,
                PriceAmountByn = entry.PriceAmountByn,
                PricePerSqmByn = listing.AreaTotal > 0m && entry.PriceAmountByn.HasValue
                    ? decimal.Round(entry.PriceAmountByn.Value / listing.AreaTotal, 2)
                    : null,
                Comment = entry.Comment
            }).ToArray()
        });
    }

    private static decimal? CalculatePercentChange(decimal? first, decimal? last)
    {
        if (!first.HasValue || !last.HasValue || first.Value <= 0m)
        {
            return null;
        }

        return decimal.Round(((last.Value - first.Value) / first.Value) * 100m, 2);
    }
}
