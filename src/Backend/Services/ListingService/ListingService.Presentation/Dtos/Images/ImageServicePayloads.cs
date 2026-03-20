namespace ListingService.Presentation.Dtos.Images;

public sealed class ImageServiceApiResult<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public sealed class UploadedImageDto
{
    public string Url { get; set; } = string.Empty;
}
