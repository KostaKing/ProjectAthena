namespace ProjectAthena.Dtos.Students;

public record UpdateStudentDto
{
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? EmergencyContact { get; init; }
    public string? EmergencyContactPhone { get; init; }
}