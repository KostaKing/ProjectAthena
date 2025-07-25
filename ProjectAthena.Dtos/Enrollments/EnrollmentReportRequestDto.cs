using ProjectAthena.Data.Models;

namespace ProjectAthena.Dtos.Enrollments;

public record EnrollmentReportRequestDto
{
    public Guid? CourseId { get; init; }
    public string? StudentId { get; init; }
    public string? InstructorId { get; init; }
    public EnrollmentStatus? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? MinGrade { get; init; }
    public decimal? MaxGrade { get; init; }
    public ReportFormat Format { get; init; } = ReportFormat.Json;
    public ReportGroupBy GroupBy { get; init; } = ReportGroupBy.Course;
}

public enum ReportFormat
{
    Json = 1,
    Csv = 2,
    Pdf = 3
}

public enum ReportGroupBy
{
    Course = 1,
    Student = 2,
    Instructor = 3,
    Status = 4,
    Date = 5
}