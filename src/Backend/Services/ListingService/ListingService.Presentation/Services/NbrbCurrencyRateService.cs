using System.Text.Json;
using System.Text.Json.Serialization;
using ListingService.Application.Contracts.Services;
using Microsoft.Extensions.Options;

namespace ListingService.Presentation.Services;

public sealed class NbrbCurrencyRateService(
    HttpClient httpClient,
    IOptions<CurrencyRateOptions> options,
    ILogger<NbrbCurrencyRateService> logger)
    : ICurrencyRateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly CurrencyRateOptions _options = options.Value;
    private readonly ILogger<NbrbCurrencyRateService> _logger = logger;

    public async Task<CurrencyConversionResult> ConvertBynToUsdAsync(decimal amountByn, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(_options.UsdRatePath, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            var rateResponse = JsonSerializer.Deserialize<NbrbRateResponse>(payload, JsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize NBRB currency rate response.");

            if (rateResponse.CurOfficialRate <= 0)
            {
                throw new InvalidOperationException("NBRB returned invalid USD official rate.");
            }

            return BuildResult(amountByn, rateResponse.CurOfficialRate, DateOnly.FromDateTime(rateResponse.Date.Date));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogWarning(
                exception,
                "Falling back to configured USD rate because NBRB is unavailable. FallbackRate: {FallbackRate}",
                _options.FallbackUsdOfficialRate);

            if (_options.FallbackUsdOfficialRate <= 0)
            {
                throw;
            }

            return BuildResult(amountByn, _options.FallbackUsdOfficialRate, DateOnly.FromDateTime(DateTime.UtcNow));
        }
    }

    private static CurrencyConversionResult BuildResult(decimal amountByn, decimal officialRate, DateOnly rateDate)
    {
        var amountUsd = decimal.Round(amountByn / officialRate, 2, MidpointRounding.AwayFromZero);
        return new CurrencyConversionResult(amountByn, amountUsd, officialRate, rateDate);
    }

    private sealed class NbrbRateResponse
    {
        [JsonPropertyName("Date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("Cur_OfficialRate")]
        public decimal CurOfficialRate { get; set; }
    }
}
