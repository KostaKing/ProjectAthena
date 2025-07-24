using ProjectAthena.Data.Models;
using ProjectAthena.Dtos.Enrollments;

namespace ProjectAthena.Dtos.Mappings;

public static class EnrollmentMappingExtensions
{
    public static EnrollmentDto ToDto(this Enrollment enrollment)
    {
        return new EnrollmentDto
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student?.FullName ?? string.Empty,
            StudentEmail = enrollment.Student?.Email ?? string.Empty,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course?.Title ?? string.Empty,
            CourseCode = enrollment.Course?.CourseCode ?? string.Empty,
            EnrollmentDate = enrollment.EnrollmentDate,
            Status = enrollment.Status,
            Grade = enrollment.Grade,
            CompletionDate = enrollment.CompletionDate,
            CreatedAt = enrollment.CreatedAt,
            UpdatedAt = enrollment.UpdatedAt,
            IsActive = enrollment.IsActive
        };
    }

    public static Enrollment ToEntity(this CreateEnrollmentDto dto)
    {
        return new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            EnrollmentDate = dto.EnrollmentDate ?? DateTime.UtcNow,
            Status = Data.Models.EnrollmentStatus.Active
        };
    }

    public static EnrollmentSummaryDto ToSummaryDto(this Enrollment enrollment)
    {
        return new EnrollmentSummaryDto
        {
            StudentName = enrollment.Student?.FullName ?? string.Empty,
            StudentEmail = enrollment.Student?.Email ?? string.Empty,
            EnrollmentDate = enrollment.EnrollmentDate,
            Status = enrollment.Status,
            Grade = enrollment.Grade,
            CompletionDate = enrollment.CompletionDate
        };
    }
}