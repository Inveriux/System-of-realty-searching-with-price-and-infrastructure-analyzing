namespace UserService.DTOs;

public sealed class UploadAvatarRequest
{
    public IFormFile? Image { get; set; }
}
