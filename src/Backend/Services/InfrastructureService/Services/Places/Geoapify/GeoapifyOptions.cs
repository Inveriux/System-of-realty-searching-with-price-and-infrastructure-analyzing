namespace InfrastructureService.Services.Places.Geoapify;

public sealed class GeoapifyOptions
{
    public const string SectionName = "Geoapify";

    public string BaseUrl { get; set; } = "https://api.geoapify.com/";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public string Language { get; set; } = "ru";
}
