namespace UserService.Services.JwtProviders.Abstractions;

public interface IRefreshTokenProvider
{
    string GenerateRefreshToken();
    string HashToken(string token);
}