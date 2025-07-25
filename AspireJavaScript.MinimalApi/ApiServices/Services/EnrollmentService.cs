using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Dtos.Enrollments;
using ProjectAthena.Dtos.Common;
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

    public async Task<PagedResult<EnrollmentDto>> GetAllEnrollmentsAsync(string? search = null, int? status = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.IsActive);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(e => 
                    e.Student.User.FirstName.ToLower().Contains(searchLower) ||
                    e.Student.User.LastName.ToLower().Contains(searchLower) ||
                    e.Student.User.Email.ToLower().Contains(searchLower) ||
                    e.Course.Title.ToLower().Contains(searchLower) ||
                    e.Course.CourseCode.ToLower().Contains(searchLower));
            }

            // Apply status filter
            if (status.HasValue)
            {
                query = query.Where(e => e.Status == (EnrollmentStatus)status.Value);
            }

            var totalCount = await query.CountAsync();
            
            var enrollments = await query
                .OrderBy(e => e.EnrollmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<EnrollmentDto>
            {
                Items = enrollments.Select(e => e.ToDto()),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments with search: {Search}, status: {Status}", search, status);
            throw;
        }
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudentIdAsync(string studentId)
    {
        try
        {
            var studentGuid = Guid.Parse(studentId);
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentGuid && e.IsActive)
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
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId && e.IsActive)
                .OrderBy(e => e.Student.User.LastName)
                .ThenBy(e => e.Student.User.FirstName)
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
                    .ThenInclude(s => s.User)
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
                    .ThenInclude(s => s.User)
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
            var studentGuid = Guid.Parse(studentId);
            return await _context.Enrollments.AnyAsync(e => 
                e.StudentId == studentGuid && 
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