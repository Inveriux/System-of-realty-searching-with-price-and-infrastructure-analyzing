namespace ListingService.Presentation.Services;

public sealed class CurrencyRateOptions
{
    public const string SectionName = "CurrencyRate";

    public string BaseUrl { get; set; } = "https://api.nbrb.by/";
    public string UsdRatePath { get; set; } = "exrates/rates/USD?parammode=2";
    public decimal FallbackUsdOfficialRate { get; set; } = 3.2m;
}
