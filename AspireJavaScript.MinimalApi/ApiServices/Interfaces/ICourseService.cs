using ProjectAthena.Dtos.Courses;

namespace AspireJavaScript.MinimalApi.ApiServices.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto?> GetCourseByIdAsync(Guid id);
    Task<CourseDto?> GetCourseByCodeAsync(string courseCode);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<CourseDto?> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto);
    Task<bool> DeleteCourseAsync(Guid id);
    Task<bool> CourseExistsAsync(Guid id);
    Task<bool> CourseCodeExistsAsync(string courseCode);
}