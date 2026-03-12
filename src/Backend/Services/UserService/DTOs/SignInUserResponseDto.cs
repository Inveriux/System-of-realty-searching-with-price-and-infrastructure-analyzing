namespace UserService.DTOs;

public record SignInUserResponseDto(
    bool Success, 
    string Token,
    string RefreshToken);