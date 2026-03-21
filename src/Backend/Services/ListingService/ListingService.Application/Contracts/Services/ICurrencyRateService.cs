namespace ListingService.Application.Contracts.Services;

public interface ICurrencyRateService
{
    Task<CurrencyConversionResult> ConvertBynToUsdAsync(decimal amountByn, CancellationToken cancellationToken);
}

public sealed record CurrencyConversionResult(
    decimal AmountByn,
    decimal AmountUsd,
    decimal BynPerUsdRate,
    DateOnly RateDate);
