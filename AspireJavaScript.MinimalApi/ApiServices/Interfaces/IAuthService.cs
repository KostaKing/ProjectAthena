using ProjectAthena.Dtos.Auth;

namespace ProjectAthena.MinimalApi.ApiServices.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> ActivateUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}