namespace ProjectAthena.Dtos.Auth;

public class LoginResponseDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required UserDto User { get; set; }
    public DateTime ExpiresAt { get; set; }
}