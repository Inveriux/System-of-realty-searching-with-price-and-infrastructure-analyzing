namespace SearchService.Services.InternalClients;

public sealed class ListingServiceClientOptions
{
    public const string SectionName = "ListingService";
    public string BaseUrl { get; set; } = "http://listing-service-svc:8080";
}

public sealed class InfrastructureServiceClientOptions
{
    public const string SectionName = "InfrastructureService";
    public string BaseUrl { get; set; } = "http://infrastructure-service-svc:8080";
}
