namespace ImageService.DTOs;

public sealed class UploadImageRequest
{
    public IFormFile? Image { get; set; }
    public string? Name { get; set; }
    public int? Expiration { get; set; }
}
