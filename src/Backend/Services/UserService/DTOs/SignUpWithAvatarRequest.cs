namespace UserService.DTOs;

public sealed class SignUpWithAvatarRequest
{
    public string User { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
