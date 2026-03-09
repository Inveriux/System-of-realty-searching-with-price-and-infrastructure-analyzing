namespace UserService.DTOs;

public record SignInUserDto(
    string Email,
    string Password
);