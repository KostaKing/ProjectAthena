namespace ProjectAthena.Dtos.Courses;

public record CourseDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string CourseCode { get; init; }
    public int Credits { get; init; }
    public string? InstructorId { get; init; }
    public string? InstructorName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int MaxEnrollments { get; init; }
    public int CurrentEnrollments { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsActive { get; init; }
}