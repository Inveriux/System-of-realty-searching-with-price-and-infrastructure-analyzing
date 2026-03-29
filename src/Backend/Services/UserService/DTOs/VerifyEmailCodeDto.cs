namespace UserService.DTOs;

public sealed record VerifyEmailCodeDto(
    string Email,
    string Code
);
