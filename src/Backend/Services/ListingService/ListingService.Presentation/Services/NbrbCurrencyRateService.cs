using System.Text.Json;
using System.Text.Json.Serialization;
using ListingService.Application.Contracts.Services;
using Microsoft.Extensions.Options;

namespace ListingService.Presentation.Services;

public sealed class NbrbCurrencyRateService(HttpClient httpClient, IOptions<CurrencyRateOptions> options)
    : ICurrencyRateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly CurrencyRateOptions _options = options.Value;

    public async Task<CurrencyConversionResult> ConvertBynToUsdAsync(decimal amountByn, CancellationToken cancellationToken)
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

        var amountUsd = decimal.Round(amountByn / rateResponse.CurOfficialRate, 2, MidpointRounding.AwayFromZero);
        var rateDate = DateOnly.FromDateTime(rateResponse.Date.Date);

        return new CurrencyConversionResult(amountByn, amountUsd, rateResponse.CurOfficialRate, rateDate);
    }

    private sealed class NbrbRateResponse
    {
        [JsonPropertyName("Date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("Cur_OfficialRate")]
        public decimal CurOfficialRate { get; set; }
    }
}
