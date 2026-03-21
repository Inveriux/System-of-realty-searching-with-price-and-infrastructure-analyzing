namespace InfrastructureService.Services.Places.Overpass;

public sealed class OverpassOptions
{
    public const string SectionName = "Overpass";

    public string BaseUrl { get; set; } = "https://overpass-api.de/api/";
    public int TimeoutSeconds { get; set; } = 20;
    public int QueryTimeoutSeconds { get; set; } = 25;
    public string UserAgent { get; set; } = "DiplomaInfrastructureService/1.0";
}
