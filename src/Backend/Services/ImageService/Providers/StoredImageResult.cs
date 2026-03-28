namespace ImageService.Providers;

public sealed class StoredImageResult
{
    public string Provider { get; init; } = "ImageKit";
    public string ProviderFileId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string? FilePath { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}
