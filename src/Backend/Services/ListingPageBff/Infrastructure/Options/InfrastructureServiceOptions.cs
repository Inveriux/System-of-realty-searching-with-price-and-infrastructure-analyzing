namespace ListingPageBff.Infrastructure.Options;

public sealed class InfrastructureServiceOptions
{
    public const string SectionName = "InfrastructureService";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 10;
}
