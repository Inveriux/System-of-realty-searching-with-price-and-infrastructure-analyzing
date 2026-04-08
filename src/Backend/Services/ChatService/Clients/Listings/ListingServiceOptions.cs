namespace ChatService.Clients.Listings;

public sealed class ListingServiceOptions
{
    public const string SectionName = "ListingService";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 10;
}
