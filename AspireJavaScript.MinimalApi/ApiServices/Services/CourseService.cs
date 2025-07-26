using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Dtos.Courses;
using ProjectAthena.Dtos.Mappings;

namespace AspireJavaScript.MinimalApi.ApiServices.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ApplicationDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments.Where(e => e.IsActive))
            .Where(c => c.IsActive)
            .OrderBy(c => c.CourseCode)
            .ToListAsync();

        return courses.Select(c => c.ToDto());
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments.Where(e => e.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        return course?.ToDto();
    }

    public async Task<CourseDto?> GetCourseByCodeAsync(string courseCode)
    {
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments.Where(e => e.IsActive))
            .FirstOrDefaultAsync(c => c.CourseCode == courseCode && c.IsActive);

        return course?.ToDto();
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        if (await CourseCodeExistsAsync(createCourseDto.CourseCode))
        {
            throw new InvalidOperationException($"Course with code '{createCourseDto.CourseCode}' already exists.");
        }

        var course = createCourseDto.ToEntity();
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return await GetCourseByIdAsync(course.Id) ?? throw new InvalidOperationException("Failed to retrieve created course.");
    }

    public async Task<CourseDto?> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (course == null)
        {
            return null;
        }

        updateCourseDto.UpdateEntity(course);
        await _context.SaveChangesAsync();

        return await GetCourseByIdAsync(id);
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (course == null)
        {
            return false;
        }

        course.IsActive = false;
        course.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CourseExistsAsync(Guid id)
    {
        return await _context.Courses.AnyAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<bool> CourseCodeExistsAsync(string courseCode)
    {
        return await _context.Courses.AnyAsync(c => c.CourseCode == courseCode && c.IsActive);
    }
}