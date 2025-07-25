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
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Prevent excessive page sizes

            var query = _context.Enrollments
                .AsNoTracking() // Performance optimization for read-only operations
                .Where(e => e.IsActive);


            // Apply status filter
            if (status.HasValue && Enum.IsDefined(typeof(EnrollmentStatus), status.Value))
            {
                query = query.Where(e => e.Status == (EnrollmentStatus)status.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            
            // Apply pagination and includes
            var enrollments = await query
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollmentDate) // More recent first
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
            _logger.LogError(ex, "Error retrieving enrollments with search: {Search}, status: {Status}, page: {Page}, pageSize: {PageSize}", 
                search, status, page, pageSize);
            throw;
        }
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudentIdAsync(string studentId)
    {
        try
        {
            // Validate and parse student ID
            if (string.IsNullOrEmpty(studentId) || !Guid.TryParse(studentId, out var studentGuid))
            {
                throw new ArgumentException("Invalid student ID", nameof(studentId));
            }

            var enrollments = await _context.Enrollments
                .AsNoTracking() // Performance optimization for read-only operations
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentGuid && e.IsActive)
                .OrderByDescending(e => e.EnrollmentDate) // More recent first
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
            // Validate course ID
            if (courseId == Guid.Empty)
            {
                throw new ArgumentException("Course ID cannot be empty", nameof(courseId));
            }

            var enrollments = await _context.Enrollments
                .AsNoTracking() // Performance optimization for read-only operations
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
            // Validate enrollment ID
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Enrollment ID cannot be empty", nameof(id));
            }

            var enrollment = await _context.Enrollments
                .AsNoTracking() // Performance optimization for read-only operations
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
            // Input validation
            if (!ValidateReportRequest(request, out var validationErrors))
            {
                throw new ArgumentException($"Invalid request parameters: {string.Join(", ", validationErrors)}");
            }

            // Build optimized query with selective loading
            var query = _context.Enrollments
                .AsNoTracking() // Performance optimization for read-only operations
                .Where(e => e.IsActive);

            // Apply filters with null checks and optimized predicates
            query = ApplyFilters(query, request);

            // Use a single query with projection to avoid N+1 problems
            var enrollmentData = await query
                .Select(e => new
                {
                    Enrollment = e,
                    StudentUser = e.Student.User,
                    Course = e.Course,
                    Instructor = e.Course.Instructor
                })
                .ToListAsync();

            if (!enrollmentData.Any())
            {
                _logger.LogInformation("No enrollments found for the given criteria");
                return CreateEmptyReport(request);
            }

            // Generate report items with optimized mapping
            var items = enrollmentData.Select(data => new EnrollmentReportItemDto
            {
                EnrollmentId = data.Enrollment.Id,
                StudentId = data.StudentUser.Id,
                StudentName = data.StudentUser.FullName ?? $"{data.StudentUser.FirstName} {data.StudentUser.LastName}",
                StudentEmail = data.StudentUser.Email ?? string.Empty,
                StudentNumber = data.Enrollment.Student.StudentNumber,
                CourseId = data.Enrollment.CourseId,
                CourseCode = data.Course.CourseCode ?? string.Empty,
                CourseTitle = data.Course.Title ?? string.Empty,
                InstructorId = data.Course.InstructorId ?? string.Empty,
                InstructorName = data.Instructor?.FullName ?? "No Instructor",
                EnrollmentDate = data.Enrollment.EnrollmentDate,
                Status = data.Enrollment.Status,
                Grade = data.Enrollment.Grade,
                CompletionDate = data.Enrollment.CompletionDate,
                CreatedAt = data.Enrollment.CreatedAt,
                UpdatedAt = data.Enrollment.UpdatedAt
            }).ToList();

            // Generate summary with proper collections
            var grades = items.Where(e => e.Grade.HasValue).Select(e => e.Grade!.Value).ToList();
            var summary = new EnrollmentReportSummaryDto
            {
                TotalEnrollments = items.Count,
                ActiveEnrollments = items.Count(e => e.Status == EnrollmentStatus.Active),
                CompletedEnrollments = items.Count(e => e.Status == EnrollmentStatus.Completed),
                DroppedEnrollments = items.Count(e => e.Status == EnrollmentStatus.Dropped),
                SuspendedEnrollments = items.Count(e => e.Status == EnrollmentStatus.Suspended),
                AverageGrade = grades.Any() ? grades.Average() : null,
                HighestGrade = grades.Any() ? grades.Max() : null,
                LowestGrade = grades.Any() ? grades.Min() : null,
                UniqueCourses = items.Select(e => e.CourseId).Distinct().Count(),
                UniqueStudents = items.Select(e => e.StudentId).Distinct().Count(),
                UniqueInstructors = items.Where(e => !string.IsNullOrEmpty(e.InstructorId))
                    .Select(e => e.InstructorId).Distinct().Count()
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

    /// <summary>
    /// Validates the enrollment report request parameters
    /// </summary>
    private bool ValidateReportRequest(EnrollmentReportRequestDto request, out List<string> errors)
    {
        errors = new List<string>();

        // Date range validation
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            if (request.StartDate.Value > request.EndDate.Value)
            {
                errors.Add("Start date cannot be after end date");
            }
            
            if (request.StartDate.Value > DateTime.UtcNow)
            {
                errors.Add("Start date cannot be in the future");
            }
        }

        // Grade range validation
        if (request.MinGrade.HasValue && request.MaxGrade.HasValue)
        {
            if (request.MinGrade.Value > request.MaxGrade.Value)
            {
                errors.Add("Minimum grade cannot be greater than maximum grade");
            }
        }

        // Grade bounds validation
        if (request.MinGrade.HasValue && (request.MinGrade.Value < 0 || request.MinGrade.Value > 100))
        {
            errors.Add("Minimum grade must be between 0 and 100");
        }

        if (request.MaxGrade.HasValue && (request.MaxGrade.Value < 0 || request.MaxGrade.Value > 100))
        {
            errors.Add("Maximum grade must be between 0 and 100");
        }

        // GUID validation for IDs
        if (!string.IsNullOrEmpty(request.StudentId) && !Guid.TryParse(request.StudentId, out _))
        {
            errors.Add("StudentId must be a valid GUID");
        }

        if (!string.IsNullOrEmpty(request.InstructorId) && !Guid.TryParse(request.InstructorId, out _))
        {
            errors.Add("InstructorId must be a valid GUID");
        }

        if (request.CourseId.HasValue && request.CourseId.Value == Guid.Empty)
        {
            errors.Add("CourseId cannot be empty GUID");
        }


        return !errors.Any();
    }

    /// <summary>
    /// Applies filters to the enrollment query with optimized predicates
    /// </summary>
    private IQueryable<Enrollment> ApplyFilters(IQueryable<Enrollment> query, EnrollmentReportRequestDto request)
    {
        // Course filter
        if (request.CourseId.HasValue)
        {
            query = query.Where(e => e.CourseId == request.CourseId.Value);
        }

        // Student filter
        if (!string.IsNullOrEmpty(request.StudentId) && Guid.TryParse(request.StudentId, out var studentGuid))
        {
            query = query.Where(e => e.StudentId == studentGuid);
        }

        // Instructor filter
        if (!string.IsNullOrEmpty(request.InstructorId) && Guid.TryParse(request.InstructorId, out var instructorGuid))
        {
            query = query.Where(e => e.Course.InstructorId == instructorGuid.ToString());
        }

        // Status filter
        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        // Date range filters
        if (request.StartDate.HasValue)
        {
            var startDate = request.StartDate.Value.Date;
            query = query.Where(e => e.EnrollmentDate.Date >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = request.EndDate.Value.Date.AddDays(1); // Include end date
            query = query.Where(e => e.EnrollmentDate.Date < endDate);
        }

        // Grade range filters
        if (request.MinGrade.HasValue)
        {
            query = query.Where(e => e.Grade.HasValue && e.Grade.Value >= request.MinGrade.Value);
        }

        if (request.MaxGrade.HasValue)
        {
            query = query.Where(e => e.Grade.HasValue && e.Grade.Value <= request.MaxGrade.Value);
        }


        // Add includes for related data
        query = query.Include(e => e.Student)
                     .ThenInclude(s => s.User)
                     .Include(e => e.Course)
                     .ThenInclude(c => c.Instructor);

        return query;
    }

    /// <summary>
    /// Creates an empty report response when no data is found
    /// </summary>
    private EnrollmentReportResponseDto CreateEmptyReport(EnrollmentReportRequestDto request)
    {
        return new EnrollmentReportResponseDto
        {
            Title = GenerateReportTitle(request),
            GeneratedAt = DateTime.UtcNow,
            Parameters = request,
            Summary = new EnrollmentReportSummaryDto
            {
                TotalEnrollments = 0,
                ActiveEnrollments = 0,
                CompletedEnrollments = 0,
                DroppedEnrollments = 0,
                SuspendedEnrollments = 0,
                AverageGrade = null,
                HighestGrade = null,
                LowestGrade = null,
                UniqueCourses = 0,
                UniqueStudents = 0,
                UniqueInstructors = 0
            },
            Groups = new List<EnrollmentReportGroupDto>(),
            Items = new List<EnrollmentReportItemDto>()
        };
    }

}