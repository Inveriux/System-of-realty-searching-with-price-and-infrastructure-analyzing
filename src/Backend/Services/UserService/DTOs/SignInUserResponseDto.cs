namespace UserService.DTOs;

public record SignInUserResponseDto(
    bool Success, 
    string AccessToken);