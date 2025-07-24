namespace ProjectAthena.Dtos.Teachers;

public record UpdateTeacherDto
{
    public string? Department { get; init; }
    public string? Title { get; init; }
    public string? Qualifications { get; init; }
    public string? Specialization { get; init; }
    public string? Phone { get; init; }
    public string? OfficeLocation { get; init; }
}