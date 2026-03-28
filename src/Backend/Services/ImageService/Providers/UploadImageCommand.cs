namespace ImageService.Providers;

public sealed class UploadImageCommand
{
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required string Folder { get; init; }
}
