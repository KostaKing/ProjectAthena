namespace ProjectAthena.Dtos.Students;

public record StudentDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? EmergencyContact { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public DateTime EnrollmentDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsActive { get; init; }
}