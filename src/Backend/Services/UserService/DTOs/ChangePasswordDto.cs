namespace UserService.DTOs;

public record ChangePasswordDto(
    string Email,
    string CurrentPassword,
    string NewPassword
);