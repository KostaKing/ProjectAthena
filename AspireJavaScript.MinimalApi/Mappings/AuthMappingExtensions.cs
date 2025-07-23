using ProjectAthena.Data.Models;
using ProjectAthena.Dtos.Auth;
using DtoUserRole = ProjectAthena.Dtos.Enums.UserRole;
using DataUserRole = ProjectAthena.Data.Models.UserRole;

namespace ProjectAthena.MinimalApi.Mappings;

public static class AuthMappingExtensions
{
    public static UserDto ToDto(this ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            Role = (DtoUserRole)(int)user.Role, // Convert enum
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            FullName = user.FullName
        };
    }

    public static ApplicationUser ToEntity(this RegisterRequestDto dto)
    {
        return new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            Role = (DataUserRole)(int)dto.Role, // Convert enum
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}