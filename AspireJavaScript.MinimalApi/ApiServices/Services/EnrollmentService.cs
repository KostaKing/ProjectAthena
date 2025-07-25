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

            var studentId = Guid.Parse(createEnrollmentDto.StudentId);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId && s.IsActive);
            if (student == null)
            {
                throw new ArgumentException("Student not found.");
            }

            // Check for duplicate enrollment
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == createEnrollmentDto.CourseId && e.IsActive);
            if (existingEnrollment != null)
            {
                throw new InvalidOperationException("Student is already enrolled in this course.");
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

    public async Task<EnrollmentReportResponseDto> GenerateAdvancedEnrollmentReportAsync(EnrollmentReportRequestDto request)
    {
        try
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(e => e.IsActive);

            // Apply filters
            if (request.CourseId.HasValue)
                query = query.Where(e => e.CourseId == request.CourseId.Value);

            if (!string.IsNullOrEmpty(request.StudentId))
            {
                var studentGuid = Guid.Parse(request.StudentId);
                query = query.Where(e => e.StudentId == studentGuid);
            }

            if (!string.IsNullOrEmpty(request.InstructorId))
            {
                query = query.Where(e => e.Course.InstructorId == request.InstructorId);
            }

            if (request.Status.HasValue)
                query = query.Where(e => e.Status == request.Status.Value);

            if (request.StartDate.HasValue)
                query = query.Where(e => e.EnrollmentDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(e => e.EnrollmentDate <= request.EndDate.Value);

            if (request.MinGrade.HasValue)
                query = query.Where(e => e.Grade >= request.MinGrade.Value);

            if (request.MaxGrade.HasValue)
                query = query.Where(e => e.Grade <= request.MaxGrade.Value);

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(e => 
                    e.Student.User.FirstName.ToLower().Contains(searchLower) ||
                    e.Student.User.LastName.ToLower().Contains(searchLower) ||
                    e.Student.User.Email.ToLower().Contains(searchLower) ||
                    e.Course.Title.ToLower().Contains(searchLower) ||
                    e.Course.CourseCode.ToLower().Contains(searchLower));
            }

            var enrollments = await query.ToListAsync();

            // Generate report items
            var items = enrollments.Select(e => new EnrollmentReportItemDto
            {
                EnrollmentId = e.Id,
                StudentId = e.Student.User.Id,
                StudentName = e.Student.User.FullName ?? $"{e.Student.User.FirstName} {e.Student.User.LastName}",
                StudentEmail = e.Student.User.Email ?? string.Empty,
                StudentNumber = e.Student.StudentNumber,
                CourseId = e.CourseId,
                CourseCode = e.Course.CourseCode ?? string.Empty,
                CourseTitle = e.Course.Title ?? string.Empty,
                InstructorId = e.Course.InstructorId ?? string.Empty,
                InstructorName = e.Course.Instructor?.FullName ?? "No Instructor",
                EnrollmentDate = e.EnrollmentDate,
                Status = e.Status,
                Grade = e.Grade,
                CompletionDate = e.CompletionDate,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            }).ToList();

            // Generate summary
            var grades = enrollments.Where(e => e.Grade.HasValue).Select(e => e.Grade!.Value).ToList();
            var summary = new EnrollmentReportSummaryDto
            {
                TotalEnrollments = enrollments.Count,
                ActiveEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                CompletedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
                DroppedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped),
                SuspendedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Suspended),
                AverageGrade = grades.Any() ? grades.Average() : null,
                HighestGrade = grades.Any() ? grades.Max() : null,
                LowestGrade = grades.Any() ? grades.Min() : null,
                UniqueCourses = enrollments.Select(e => e.CourseId).Distinct().Count(),
                UniqueStudents = enrollments.Select(e => e.StudentId).Distinct().Count(),
                UniqueInstructors = enrollments.Where(e => !string.IsNullOrEmpty(e.Course.InstructorId))
                    .Select(e => e.Course.InstructorId).Distinct().Count()
            };

            // Generate groups based on GroupBy parameter
            var groups = GenerateReportGroups(items, request.GroupBy);

            // Generate title
            var title = GenerateReportTitle(request);

            return new EnrollmentReportResponseDto
            {
                Title = title,
                GeneratedAt = DateTime.UtcNow,
                Parameters = request,
                Summary = summary,
                Groups = groups,
                Items = items
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced enrollment report");
            throw;
        }
    }

    private List<EnrollmentReportGroupDto> GenerateReportGroups(List<EnrollmentReportItemDto> items, ReportGroupBy groupBy)
    {
        return groupBy switch
        {
            ReportGroupBy.Course => items
                .GroupBy(i => new { i.CourseId, i.CourseCode, i.CourseTitle })
                .Select(g => new EnrollmentReportGroupDto
                {
                    GroupKey = g.Key.CourseId.ToString(),
                    GroupLabel = $"{g.Key.CourseCode} - {g.Key.CourseTitle}",
                    Count = g.Count(),
                    AverageGrade = g.Where(i => i.Grade.HasValue).Any() ? g.Where(i => i.Grade.HasValue).Average(i => i.Grade!.Value) : null,
                    Items = g.ToList()
                }).ToList(),

            ReportGroupBy.Student => items
                .GroupBy(i => new { i.StudentId, i.StudentName, i.StudentEmail })
                .Select(g => new EnrollmentReportGroupDto
                {
                    GroupKey = g.Key.StudentId,
                    GroupLabel = $"{g.Key.StudentName} ({g.Key.StudentEmail})",
                    Count = g.Count(),
                    AverageGrade = g.Where(i => i.Grade.HasValue).Any() ? g.Where(i => i.Grade.HasValue).Average(i => i.Grade!.Value) : null,
                    Items = g.ToList()
                }).ToList(),

            ReportGroupBy.Instructor => items
                .GroupBy(i => new { i.InstructorId, i.InstructorName })
                .Select(g => new EnrollmentReportGroupDto
                {
                    GroupKey = g.Key.InstructorId,
                    GroupLabel = g.Key.InstructorName,
                    Count = g.Count(),
                    AverageGrade = g.Where(i => i.Grade.HasValue).Any() ? g.Where(i => i.Grade.HasValue).Average(i => i.Grade!.Value) : null,
                    Items = g.ToList()
                }).ToList(),

            ReportGroupBy.Status => items
                .GroupBy(i => i.Status)
                .Select(g => new EnrollmentReportGroupDto
                {
                    GroupKey = ((int)g.Key).ToString(),
                    GroupLabel = g.Key.ToString(),
                    Count = g.Count(),
                    AverageGrade = g.Where(i => i.Grade.HasValue).Any() ? g.Where(i => i.Grade.HasValue).Average(i => i.Grade!.Value) : null,
                    Items = g.ToList()
                }).ToList(),

            ReportGroupBy.Date => items
                .GroupBy(i => i.EnrollmentDate.Date)
                .Select(g => new EnrollmentReportGroupDto
                {
                    GroupKey = g.Key.ToString("yyyy-MM-dd"),
                    GroupLabel = g.Key.ToString("MMMM dd, yyyy"),
                    Count = g.Count(),
                    AverageGrade = g.Where(i => i.Grade.HasValue).Any() ? g.Where(i => i.Grade.HasValue).Average(i => i.Grade!.Value) : null,
                    Items = g.ToList()
                }).ToList(),

            _ => new List<EnrollmentReportGroupDto>()
        };
    }

    private string GenerateReportTitle(EnrollmentReportRequestDto request)
    {
        var parts = new List<string> { "Enrollment Report" };

        if (request.CourseId.HasValue)
            parts.Add("for Specific Course");
        
        if (!string.IsNullOrEmpty(request.StudentId))
            parts.Add("for Specific Student");

        if (!string.IsNullOrEmpty(request.InstructorId))
            parts.Add("for Specific Instructor");

        if (request.Status.HasValue)
            parts.Add($"- {request.Status.Value} Only");

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue)
                parts.Add($"({request.StartDate.Value:MMM yyyy} - {request.EndDate.Value:MMM yyyy})");
            else if (request.StartDate.HasValue)
                parts.Add($"(From {request.StartDate.Value:MMM yyyy})");
            else if (request.EndDate.HasValue)
                parts.Add($"(Until {request.EndDate.Value:MMM yyyy})");
        }

        return string.Join(" ", parts);
    }
}