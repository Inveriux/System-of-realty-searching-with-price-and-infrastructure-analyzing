namespace ListingService.Presentation.Services;

public sealed class ImageServiceOptions
{
    public const string SectionName = "ImageService";

    public string BaseUrl { get; set; } = "http://localhost:8080";
}
