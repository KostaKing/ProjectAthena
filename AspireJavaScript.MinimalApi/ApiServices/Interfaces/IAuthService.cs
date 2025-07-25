using ProjectAthena.Dtos.Auth;

namespace ProjectAthena.MinimalApi.ApiServices.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task LogoutAsync(string userId);
    Task<UserDto> GetCurrentUserAsync(string userId);
    Task<UserDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<bool> ValidateTokenAsync(string token);
    Task<List<UserDto>> GetAllUsersAsync();
}