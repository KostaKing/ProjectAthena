using System.ComponentModel.DataAnnotations;
using ProjectAthena.Data.Models;

namespace ProjectAthena.Dtos.Enrollments;

public record EnrollmentReportValidationDto
{
    [RegularExpression(@"^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$", 
        ErrorMessage = "CourseId must be a valid GUID")]
    public Guid? CourseId { get; init; }

    [RegularExpression(@"^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$", 
        ErrorMessage = "StudentId must be a valid GUID")]
    public string? StudentId { get; init; }

    [RegularExpression(@"^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$", 
        ErrorMessage = "InstructorId must be a valid GUID")]
    public string? InstructorId { get; init; }

    [Range(1, 4, ErrorMessage = "Status must be between 1 and 4")]
    public EnrollmentStatus? Status { get; init; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; init; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; init; }

    [Range(0, 100, ErrorMessage = "MinGrade must be between 0 and 100")]
    public decimal? MinGrade { get; init; }

    [Range(0, 100, ErrorMessage = "MaxGrade must be between 0 and 100")]
    public decimal? MaxGrade { get; init; }


    [Range(1, 3, ErrorMessage = "Format must be between 1 and 3")]
    public ReportFormat Format { get; init; } = ReportFormat.Json;

    [Range(1, 5, ErrorMessage = "GroupBy must be between 1 and 5")]
    public ReportGroupBy GroupBy { get; init; } = ReportGroupBy.Course;
}

public static class EnrollmentReportValidationExtensions
{
    public static bool IsValidDateRange(this EnrollmentReportValidationDto dto)
    {
        if (dto.StartDate.HasValue && dto.EndDate.HasValue)
        {
            return dto.StartDate.Value <= dto.EndDate.Value;
        }
        return true;
    }

    public static bool IsValidGradeRange(this EnrollmentReportValidationDto dto)
    {
        if (dto.MinGrade.HasValue && dto.MaxGrade.HasValue)
        {
            return dto.MinGrade.Value <= dto.MaxGrade.Value;
        }
        return true;
    }

}