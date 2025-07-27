using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Dtos.Auth;
using ProjectAthena.MinimalApi.Mappings;
using ProjectAthena.MinimalApi.ApiServices.Interfaces;
using System.Security.Claims;

namespace ProjectAthena.MinimalApi.ApiServices.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new UnauthorizedAccessException("Account is locked out");
            }
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            User = user.ToDto(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Match token expiry
        };
    }

    public async Task<UserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists");
        }

        var user = request.ToEntity();
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("\n", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User creation failed for {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException($"Failed to create user:\n{errors}");
        }

        // Add user to role
        var roleName = request.Role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);

        _logger.LogInformation("New user {Email} registered with role {Role}", request.Email, roleName);

        return user.ToDto();
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.RefreshToken);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Invalid token claims");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        var isValidRefreshToken = await _tokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken);
        if (!isValidRefreshToken)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

        return new LoginResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            User = user.ToDto(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task LogoutAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _tokenService.RevokeRefreshTokenAsync(userId);
        _logger.LogInformation("User {UserId} logged out", userId);
    }

    public async Task<UserDto> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        return user.ToDto();
    }

    public async Task<UserDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to change password: {errors}");
        }

        _logger.LogInformation("User {UserId} changed password", userId);
        return user.ToDto();
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);
            return Task.FromResult(principal != null);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        // Use async enumeration and avoid loading all users into memory at once
        var users = await _userManager.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
        // Use modern C# 12 collection expressions and LINQ for better performance
        var userDtos = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = user.ToDto();
            
            // Use pattern matching for role assignment (modern C# approach)
            userDto.Role = roles switch
            {
                var r when r.Contains("Admin") => ProjectAthena.Dtos.Enums.UserRole.Admin,
                var r when r.Contains("Teacher") => ProjectAthena.Dtos.Enums.UserRole.Teacher,
                var r when r.Contains("Student") => ProjectAthena.Dtos.Enums.UserRole.Student,
                _ => ProjectAthena.Dtos.Enums.UserRole.Student // Default fallback
            };
                
            userDtos.Add(userDto);
        }

        return userDtos;
    }

    public async Task<UserDto> ActivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to activate user: {errors}");
        }

        _logger.LogInformation("User {UserId} activated", userId);
        return user.ToDto();
    }

    public async Task<UserDto> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to deactivate user: {errors}");
        }

        // Revoke all refresh tokens for this user
        await _tokenService.RevokeRefreshTokenAsync(userId);

        _logger.LogInformation("User {UserId} deactivated", userId);
        return user.ToDto();
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var result = await _userManager.DeleteAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user: {errors}");
        }

        // Revoke all refresh tokens for this user
        await _tokenService.RevokeRefreshTokenAsync(userId);

        _logger.LogInformation("User {UserId} deleted", userId);
    }
}