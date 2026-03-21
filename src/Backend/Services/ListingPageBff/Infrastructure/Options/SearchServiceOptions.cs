namespace ListingPageBff.Infrastructure.Options;

public sealed class SearchServiceOptions
{
    public const string SectionName = "SearchService";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 5;
}
