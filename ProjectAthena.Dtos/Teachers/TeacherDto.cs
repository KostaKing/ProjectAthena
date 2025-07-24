namespace ProjectAthena.Dtos.Teachers;

public record TeacherDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string EmployeeNumber { get; init; } = string.Empty;
    public string? Department { get; init; }
    public string? Title { get; init; }
    public string? Qualifications { get; init; }
    public string? Specialization { get; init; }
    public string? Phone { get; init; }
    public string? OfficeLocation { get; init; }
    public DateTime HireDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsActive { get; init; }
}