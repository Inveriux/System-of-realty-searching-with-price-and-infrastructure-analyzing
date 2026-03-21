namespace InfrastructureService.Services.Yandex;

public sealed class YandexOptions
{
    public const string SectionName = "Yandex";

    public string GeocoderBaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Language { get; set; } = "ru_RU";
}
