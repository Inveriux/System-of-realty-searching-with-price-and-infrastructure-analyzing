namespace InfrastructureService.Services.Routing;

public sealed class OsrmOptions
{
    public const string SectionName = "Osrm";

    public string BaseUrl { get; set; } = "http://osrm-service-svc:5000";
    public string? FootBaseUrl { get; set; }
    public string? CarBaseUrl { get; set; }
    public string FootProfile { get; set; } = "foot";
    public string CarProfile { get; set; } = "driving";
    public int RequestTimeoutSeconds { get; set; } = 8;
}
