namespace ProjectAthena.Dtos.Students;

public record CreateStudentDto
{
    public required string UserId { get; init; }
    public required string StudentNumber { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? EmergencyContact { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public DateTime? EnrollmentDate { get; init; }
}