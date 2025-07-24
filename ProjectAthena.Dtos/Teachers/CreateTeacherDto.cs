namespace ProjectAthena.Dtos.Teachers;

public record CreateTeacherDto
{
    public required string UserId { get; init; }
    public required string EmployeeNumber { get; init; }
    public string? Department { get; init; }
    public string? Title { get; init; }
    public string? Qualifications { get; init; }
    public string? Specialization { get; init; }
    public string? Phone { get; init; }
    public string? OfficeLocation { get; init; }
    public DateTime? HireDate { get; init; }
}