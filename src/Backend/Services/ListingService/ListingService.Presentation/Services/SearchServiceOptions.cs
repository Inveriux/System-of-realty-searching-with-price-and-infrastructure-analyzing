namespace ListingService.Presentation.Services;

public sealed class SearchServiceOptions
{
    public const string SectionName = "SearchService";

    public string BaseUrl { get; set; } = "http://localhost:8080";
}
