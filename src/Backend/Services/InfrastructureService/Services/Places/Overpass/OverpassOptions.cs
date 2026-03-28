namespace InfrastructureService.Services.Places.Overpass;

public sealed class OverpassOptions
{
    public const string SectionName = "Overpass";

    public string BaseUrl { get; set; } = "https://overpass-api.de/api/interpreter";
    public List<string> FallbackBaseUrls { get; set; } = [];
    public int TimeoutSeconds { get; set; } = 25;
    public int DefaultRadiusMeters { get; set; } = 1200;
    public int MaxRadiusMeters { get; set; } = 15000;
    public bool UsePublicInstance { get; set; } = true;
    public int RetryCount { get; set; } = 2;
    public int RetryDelayMs { get; set; } = 500;
    public string UserAgent { get; set; } = "DiplomaRealEstateApp/1.0";
}
