using ListingService.Application.Contracts.Repositories;
using ListingService.Application.Contracts.Services;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Enums;
using ListingService.Domain.Models;
using BasicBuildingBlocks.BusinessResults;
using MediatR;

namespace ListingService.Application.UseCases.Listings;

public sealed record UpdateListingPriceCommand(Guid ListingId, UpdateListingPriceRequestDto Request)
    : IRequest<Result<ListingPriceHistoryTimelineDto>>;

public sealed record AddListingPriceHistoryEntryCommand(Guid ListingId, CreateListingPriceHistoryEntryRequestDto Request)
    : IRequest<Result<ListingPriceHistoryTimelineDto>>;

public sealed record UpdateListingPriceHistoryEntryCommand(Guid ListingId, Guid HistoryEntryId, UpdateListingPriceHistoryEntryRequestDto Request)
    : IRequest<Result<ListingPriceHistoryTimelineDto>>;

public sealed record DeleteListingPriceHistoryEntryCommand(Guid ListingId, Guid HistoryEntryId)
    : IRequest<Result<ListingPriceHistoryTimelineDto>>;

public sealed class UpdateListingPriceHandler(
    IRepositoryManager manager,
    ICurrencyRateService currencyRateService,
    ISearchServiceClient searchServiceClient)
    : IRequestHandler<UpdateListingPriceCommand, Result<ListingPriceHistoryTimelineDto>>,
      IRequestHandler<AddListingPriceHistoryEntryCommand, Result<ListingPriceHistoryTimelineDto>>,
      IRequestHandler<UpdateListingPriceHistoryEntryCommand, Result<ListingPriceHistoryTimelineDto>>,
      IRequestHandler<DeleteListingPriceHistoryEntryCommand, Result<ListingPriceHistoryTimelineDto>>
{
    public async Task<Result<ListingPriceHistoryTimelineDto>> Handle(
        UpdateListingPriceCommand request,
        CancellationToken cancellationToken)
    {
        var listing = await manager.Listing.GetDetailedByIdAsync(request.ListingId, true, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound<ListingPriceHistoryTimelineDto>("Объявление не найдено.");
        }

        var historyEntry = await BuildHistoryEntryAsync(
            request.ListingId,
            request.Request.PriceAmountByn,
            request.Request.EffectiveDateUtc ?? DateTimeOffset.UtcNow,
            request.Request.Comment,
            cancellationToken);

        listing.ChangePrice(historyEntry);
        await manager.Listing.AddPriceHistoryEntryAsync(historyEntry, cancellationToken);
        await manager.SaveChangesAsync(cancellationToken);
        await ReindexListingAsync(request.ListingId, cancellationToken);

        return await BuildHistoryResponseAsync(request.ListingId, cancellationToken);
    }

    public async Task<Result<ListingPriceHistoryTimelineDto>> Handle(
        AddListingPriceHistoryEntryCommand request,
        CancellationToken cancellationToken)
    {
        var listing = await manager.Listing.GetDetailedByIdAsync(request.ListingId, true, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound<ListingPriceHistoryTimelineDto>("Объявление не найдено.");
        }

        var historyEntry = await BuildHistoryEntryAsync(
            request.ListingId,
            request.Request.PriceAmountByn,
            request.Request.EffectiveDateUtc,
            request.Request.Comment,
            cancellationToken);

        await manager.Listing.AddPriceHistoryEntryAsync(historyEntry, cancellationToken);
        await manager.Listing.SyncCurrentPriceWithLatestHistoryAsync(request.ListingId, cancellationToken);
        await manager.SaveChangesAsync(cancellationToken);
        await ReindexListingAsync(request.ListingId, cancellationToken);

        return await BuildHistoryResponseAsync(request.ListingId, cancellationToken);
    }

    public async Task<Result<ListingPriceHistoryTimelineDto>> Handle(
        UpdateListingPriceHistoryEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await manager.Listing.GetPriceHistoryEntryAsync(request.ListingId, request.HistoryEntryId, true, cancellationToken);
        if (entry is null)
        {
            return Result.NotFound<ListingPriceHistoryTimelineDto>("Запись истории цен не найдена.");
        }

        var listing = await manager.Listing.GetDetailedByIdAsync(request.ListingId, true, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound<ListingPriceHistoryTimelineDto>("Объявление не найдено.");
        }

        if (request.Request.PriceAmountByn.HasValue)
        {
            var converted = await currencyRateService.ConvertBynToUsdAsync(request.Request.PriceAmountByn.Value, cancellationToken);
            entry.PriceAmount = converted.AmountByn;
            entry.PriceAmountByn = converted.AmountByn;
            entry.PriceAmountUsd = converted.AmountUsd;
            entry.PriceCurrency = Currency.BYN.ToString();
        }

        if (request.Request.EffectiveDateUtc.HasValue)
        {
            entry.ChangedAt = request.Request.EffectiveDateUtc.Value.ToUniversalTime();
        }

        if (request.Request.Comment is not null)
        {
            entry.Comment = string.IsNullOrWhiteSpace(request.Request.Comment)
                ? null
                : request.Request.Comment.Trim();
        }

        await manager.Listing.SyncCurrentPriceWithLatestHistoryAsync(request.ListingId, cancellationToken);
        await manager.SaveChangesAsync(cancellationToken);
        await ReindexListingAsync(request.ListingId, cancellationToken);

        return await BuildHistoryResponseAsync(request.ListingId, cancellationToken);
    }

    public async Task<Result<ListingPriceHistoryTimelineDto>> Handle(
        DeleteListingPriceHistoryEntryCommand request,
        CancellationToken cancellationToken)
    {
        var history = await manager.Listing.GetPriceHistoryAsync(request.ListingId, cancellationToken);
        if (history.Count <= 1)
        {
            return Result.Fail<ListingPriceHistoryTimelineDto>(ResultStatusCode.Validation, "Нельзя удалить последнюю запись истории цен.");
        }

        var entry = await manager.Listing.GetPriceHistoryEntryAsync(request.ListingId, request.HistoryEntryId, true, cancellationToken);
        if (entry is null)
        {
            return Result.NotFound<ListingPriceHistoryTimelineDto>("Запись истории цен не найдена.");
        }

        manager.Listing.RemovePriceHistoryEntry(entry);
        await manager.Listing.SyncCurrentPriceWithLatestHistoryAsync(request.ListingId, cancellationToken);
        await manager.SaveChangesAsync(cancellationToken);
        await ReindexListingAsync(request.ListingId, cancellationToken);

        return await BuildHistoryResponseAsync(request.ListingId, cancellationToken);
    }

    private async Task<ListingPriceHistory> BuildHistoryEntryAsync(
        Guid listingId,
        decimal priceAmountByn,
        DateTimeOffset effectiveDateUtc,
        string? comment,
        CancellationToken cancellationToken)
    {
        var converted = await currencyRateService.ConvertBynToUsdAsync(priceAmountByn, cancellationToken);

        return new ListingPriceHistory
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            ChangedAt = effectiveDateUtc.ToUniversalTime(),
            PriceAmount = converted.AmountByn,
            PriceAmountByn = converted.AmountByn,
            PriceAmountUsd = converted.AmountUsd,
            PriceCurrency = Currency.BYN.ToString(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        };
    }

    private async Task<Result<ListingPriceHistoryTimelineDto>> BuildHistoryResponseAsync(Guid listingId, CancellationToken cancellationToken)
    {
        var listing = await manager.Listing.GetDetailedByIdAsync(listingId, false, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound<ListingPriceHistoryTimelineDto>("Объявление не найдено.");
        }

        var history = await manager.Listing.GetPriceHistoryAsync(listingId, cancellationToken);
        var orderedHistory = history.OrderBy(x => x.ChangedAt).ThenBy(x => x.Id).ToArray();
        var first = orderedHistory.FirstOrDefault();
        var last = orderedHistory.LastOrDefault();

        return Result.Ok(new ListingPriceHistoryTimelineDto
        {
            ListingId = listingId,
            CurrentPriceAmountByn = listing.PriceAmountByn,
            FirstRecordedPriceAmountByn = first?.PriceAmountByn,
            LastRecordedPriceAmountByn = last?.PriceAmountByn,
            FirstRecordedAt = first?.ChangedAt,
            LastRecordedAt = last?.ChangedAt,
            PercentChangeFromFirst = first?.PriceAmountByn is > 0m && last?.PriceAmountByn is not null
                ? decimal.Round(((last.PriceAmountByn.Value - first.PriceAmountByn.Value) / first.PriceAmountByn.Value) * 100m, 2)
                : null,
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

    private async Task ReindexListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        var listing = await manager.Listing.GetDetailedByIdAsync(listingId, false, cancellationToken);
        if (listing is null)
        {
            return;
        }

        await searchServiceClient.IndexListingAsync(new SearchListingIndexRequest
        {
            ListingId = listing.Id,
            AgentId = listing.AgentId,
            Title = listing.Title,
            Description = listing.Description,
            AddressText = listing.AddressText,
            LocationId = listing.LocationId,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            ListingType = listing.ListingType.Name,
            BuildingType = listing.BuildingType?.Name,
            PriceAmount = listing.PriceAmount,
            PriceAmountByn = listing.PriceAmountByn,
            PriceAmountUsd = listing.PriceAmountUsd,
            PriceCurrency = listing.PriceCurrency,
            AreaTotal = listing.AreaTotal,
            MainMediaUrl = listing.Media?
                .Where(static media => media.IsMainMedia)
                .Select(static media => media.Url)
                .FirstOrDefault(),
            MediaCount = listing.Media?.Count ?? 0,
            PublishedDate = listing.PublishedDate,
            ListingDetailsType = listing.ListingDetailsType
        }, cancellationToken);
    }
}
