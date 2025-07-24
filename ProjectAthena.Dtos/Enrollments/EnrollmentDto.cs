using ProjectAthena.Dtos.Auth;
using ProjectAthena.Dtos.Courses;
using ProjectAthena.Data.Models;

namespace ProjectAthena.Dtos.Enrollments;

public record EnrollmentDto
{
    public Guid Id { get; init; }
    public required string StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string CourseCode { get; init; } = string.Empty;
    public DateTime EnrollmentDate { get; init; }
    public EnrollmentStatus Status { get; init; }
    public decimal? Grade { get; init; }
    public DateTime? CompletionDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsActive { get; init; }
}