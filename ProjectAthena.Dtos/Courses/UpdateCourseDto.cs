namespace ProjectAthena.Dtos.Courses;

public record UpdateCourseDto
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public int Credits { get; init; }
    public string? InstructorId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int MaxEnrollments { get; init; }
}