namespace UserService.DTOs;

public sealed record UpdateOwnProfileDto(
    string Name,
    string? PhoneNumber
);
