namespace ImageService.DTOs;

public sealed class UploadManyImagesRequest
{
    public List<IFormFile> Images { get; set; } = [];
    public int? Expiration { get; set; }
}
