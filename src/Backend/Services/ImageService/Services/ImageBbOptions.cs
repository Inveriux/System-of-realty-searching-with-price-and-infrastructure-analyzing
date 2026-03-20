namespace ImageService.Services;

public sealed class ImageBbOptions
{
    public const string SectionName = "ImageBb";

    public string Endpoint { get; set; } = "https://api.imgbb.com/1/upload";
    public string ApiKey { get; set; } = string.Empty;
    public long MaxFileSizeInBytes { get; set; } = 10 * 1024 * 1024;
    public int MaxFilesPerRequest { get; set; } = 10;
}
