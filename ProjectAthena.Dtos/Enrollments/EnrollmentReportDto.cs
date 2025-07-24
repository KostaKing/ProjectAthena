using ProjectAthena.Data.Models;

namespace ProjectAthena.Dtos.Enrollments;

public record EnrollmentReportDto
{
    public string CourseCode { get; init; } = string.Empty;
    public string CourseTitle { get; init; } = string.Empty;
    public string InstructorName { get; init; } = string.Empty;
    public int TotalEnrollments { get; init; }
    public int ActiveEnrollments { get; init; }
    public int CompletedEnrollments { get; init; }
    public int DroppedEnrollments { get; init; }
    public decimal AverageGrade { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public List<EnrollmentSummaryDto> StudentEnrollments { get; init; } = new();
}

public record EnrollmentSummaryDto
{
    public string StudentName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public DateTime EnrollmentDate { get; init; }
    public EnrollmentStatus Status { get; init; }
    public decimal? Grade { get; init; }
    public DateTime? CompletionDate { get; init; }
}