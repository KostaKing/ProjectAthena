using ProjectAthena.Data.Models;

namespace ProjectAthena.Dtos.Enrollments;

public record EnrollmentReportResponseDto
{
    public string Title { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public EnrollmentReportRequestDto Parameters { get; init; } = new();
    public EnrollmentReportSummaryDto Summary { get; init; } = new();
    public List<EnrollmentReportGroupDto> Groups { get; init; } = new();
    public List<EnrollmentReportItemDto> Items { get; init; } = new();
}

public record EnrollmentReportSummaryDto
{
    public int TotalEnrollments { get; init; }
    public int ActiveEnrollments { get; init; }
    public int CompletedEnrollments { get; init; }
    public int DroppedEnrollments { get; init; }
    public int SuspendedEnrollments { get; init; }
    public decimal? AverageGrade { get; init; }
    public decimal? HighestGrade { get; init; }
    public decimal? LowestGrade { get; init; }
    public int UniqueCourses { get; init; }
    public int UniqueStudents { get; init; }
    public int UniqueInstructors { get; init; }
}

public record EnrollmentReportGroupDto
{
    public string GroupKey { get; init; } = string.Empty;
    public string GroupLabel { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal? AverageGrade { get; init; }
    public List<EnrollmentReportItemDto> Items { get; init; } = new();
}

public record EnrollmentReportItemDto
{
    public Guid EnrollmentId { get; init; }
    public string StudentId { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public Guid CourseId { get; init; }
    public string CourseCode { get; init; } = string.Empty;
    public string CourseTitle { get; init; } = string.Empty;
    public string InstructorId { get; init; } = string.Empty;
    public string InstructorName { get; init; } = string.Empty;
    public DateTime EnrollmentDate { get; init; }
    public EnrollmentStatus Status { get; init; }
    public decimal? Grade { get; init; }
    public DateTime? CompletionDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}