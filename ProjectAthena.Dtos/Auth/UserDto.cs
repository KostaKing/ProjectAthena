using ProjectAthena.Dtos.Enums;

namespace ProjectAthena.Dtos.Auth;

public class UserDto
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public string FullName { get; set; } = string.Empty;
}