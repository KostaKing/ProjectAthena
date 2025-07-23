using ProjectAthena.Dtos.Enums;

namespace ProjectAthena.Dtos.Auth;

public class RegisterRequestDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public UserRole Role { get; set; } = UserRole.Student;
}