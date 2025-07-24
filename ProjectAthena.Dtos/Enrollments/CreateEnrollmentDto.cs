namespace ProjectAthena.Dtos.Enrollments;

public record CreateEnrollmentDto
{
    public required string StudentId { get; init; }
    public Guid CourseId { get; init; }
    public DateTime? EnrollmentDate { get; init; }
}