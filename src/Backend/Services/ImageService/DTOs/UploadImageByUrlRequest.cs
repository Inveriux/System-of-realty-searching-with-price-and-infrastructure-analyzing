namespace ImageService.DTOs;

public sealed class UploadImageByUrlRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int? Expiration { get; set; }
}
