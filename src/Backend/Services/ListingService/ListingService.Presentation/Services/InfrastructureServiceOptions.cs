namespace ListingService.Presentation.Services;

public sealed class InfrastructureServiceOptions
{
    public const string SectionName = "InfrastructureService";

    public string BaseUrl { get; set; } = "http://localhost:8080";
}
