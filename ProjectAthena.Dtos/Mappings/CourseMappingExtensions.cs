using ProjectAthena.Data.Models;
using ProjectAthena.Dtos.Courses;

namespace ProjectAthena.Dtos.Mappings;

public static class CourseMappingExtensions
{
    public static CourseDto ToDto(this Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CourseCode = course.CourseCode,
            Credits = course.Credits,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.FullName,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            MaxEnrollments = course.MaxEnrollments,
            CurrentEnrollments = course.Enrollments?.Count(e => e.Status == Data.Models.EnrollmentStatus.Active) ?? 0,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            IsActive = course.IsActive
        };
    }

    public static Course ToEntity(this CreateCourseDto dto)
    {
        return new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            CourseCode = dto.CourseCode,
            Credits = dto.Credits,
            InstructorId = dto.InstructorId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            MaxEnrollments = dto.MaxEnrollments
        };
    }

    public static void UpdateEntity(this UpdateCourseDto dto, Course course)
    {
        course.Title = dto.Title;
        course.Description = dto.Description;
        course.Credits = dto.Credits;
        course.InstructorId = dto.InstructorId;
        course.StartDate = dto.StartDate;
        course.EndDate = dto.EndDate;
        course.MaxEnrollments = dto.MaxEnrollments;
        course.UpdatedAt = DateTime.UtcNow;
    }
}