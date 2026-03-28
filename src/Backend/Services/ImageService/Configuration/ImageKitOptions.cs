namespace ImageService.Configuration;

public sealed class ImageKitOptions
{
    public const string SectionName = "ImageKit";

    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string UrlEndpoint { get; set; } = string.Empty;
    public string UploadEndpoint { get; set; } = "https://upload.imagekit.io/api/v1/files/upload";
    public string BaseFolder { get; set; } = "/diploma";
    public bool UseUniqueFileName { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public long MaxFileSizeInBytes { get; set; } = 10 * 1024 * 1024;
    public int MaxFilesPerRequest { get; set; } = 10;
}
