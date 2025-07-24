using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Dtos.Enrollments;
using ProjectAthena.Dtos.Mappings;

namespace AspireJavaScript.MinimalApi.ApiServices.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ApplicationDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync()
    {
        try
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.IsActive)
                .OrderBy(e => e.EnrollmentDate)
                .ToListAsync();

            return enrollments.Select(e => e.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all enrollments");
            throw;
        }
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudentIdAsync(string studentId)
    {
        try
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId && e.IsActive)
                .OrderBy(e => e.EnrollmentDate)
                .ToListAsync();

            return enrollments.Select(e => e.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments for student: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseIdAsync(Guid courseId)
    {
        try
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId && e.IsActive)
                .OrderBy(e => e.Student.LastName)
                .ThenBy(e => e.Student.FirstName)
                .ToListAsync();

            return enrollments.Select(e => e.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments for course: {CourseId}", courseId);
            throw;
        }
    }

    public async Task<EnrollmentDto?> GetEnrollmentByIdAsync(Guid id)
    {
        try
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

            return enrollment?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollment with ID: {EnrollmentId}", id);
            throw;
        }
    }

    public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        try
        {
            if (await IsStudentEnrolledAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId))
            {
                throw new InvalidOperationException("Student is already enrolled in this course.");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == createEnrollmentDto.CourseId && c.IsActive);
            if (course == null)
            {
                throw new ArgumentException("Course not found.");
            }

            var currentEnrollments = await _context.Enrollments
                .CountAsync(e => e.CourseId == course.Id && e.Status == EnrollmentStatus.Active && e.IsActive);

            if (currentEnrollments >= course.MaxEnrollments)
            {
                throw new InvalidOperationException("Course has reached maximum enrollment capacity.");
            }

            var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == createEnrollmentDto.StudentId);
            if (student == null)
            {
                throw new ArgumentException("Student not found.");
            }

            var enrollment = createEnrollmentDto.ToEntity();
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return await GetEnrollmentByIdAsync(enrollment.Id) ?? throw new InvalidOperationException("Failed to retrieve created enrollment.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating enrollment for student {StudentId} in course {CourseId}", 
                createEnrollmentDto.StudentId, createEnrollmentDto.CourseId);
            throw;
        }
    }

    public async Task<bool> UpdateEnrollmentStatusAsync(Guid id, EnrollmentStatus status, decimal? grade = null)
    {
        try
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (enrollment == null)
            {
                return false;
            }

            enrollment.Status = status;
            enrollment.Grade = grade;
            enrollment.UpdatedAt = DateTime.UtcNow;

            if (status == EnrollmentStatus.Completed)
            {
                enrollment.CompletionDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating enrollment status for ID: {EnrollmentId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteEnrollmentAsync(Guid id)
    {
        try
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (enrollment == null)
            {
                return false;
            }

            enrollment.IsActive = false;
            enrollment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting enrollment with ID: {EnrollmentId}", id);
            throw;
        }
    }

    public async Task<EnrollmentReportDto?> GenerateEnrollmentReportAsync(Guid courseId)
    {
        try
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments.Where(e => e.IsActive))
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive);

            if (course == null)
            {
                return null;
            }

            var enrollments = course.Enrollments.ToList();
            var grades = enrollments.Where(e => e.Grade.HasValue).Select(e => e.Grade!.Value).ToList();

            return new EnrollmentReportDto
            {
                CourseCode = course.CourseCode,
                CourseTitle = course.Title,
                InstructorName = course.Instructor?.FullName ?? "No Instructor Assigned",
                TotalEnrollments = enrollments.Count,
                ActiveEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                CompletedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
                DroppedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped),
                AverageGrade = grades.Any() ? grades.Average() : 0,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                StudentEnrollments = enrollments.Select(e => e.ToSummaryDto()).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enrollment report for course: {CourseId}", courseId);
            throw;
        }
    }

    public async Task<bool> IsStudentEnrolledAsync(string studentId, Guid courseId)
    {
        try
        {
            return await _context.Enrollments.AnyAsync(e => 
                e.StudentId == studentId && 
                e.CourseId == courseId && 
                e.Status == EnrollmentStatus.Active && 
                e.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking enrollment status for student {StudentId} in course {CourseId}", 
                studentId, courseId);
            throw;
        }
    }
}