namespace InfrastructureService.Services.Places;

public sealed class PlacesProviderOptions
{
    public const string SectionName = "PlacesProvider";

    public string Provider { get; set; } = "Overpass";
}
