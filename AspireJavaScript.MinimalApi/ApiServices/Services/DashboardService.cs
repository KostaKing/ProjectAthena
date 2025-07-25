using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Dtos.Dashboard;

namespace AspireJavaScript.MinimalApi.ApiServices.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        try
        {
            var stats = new DashboardStatsDto();

            // Get basic counts
            stats.TotalCourses = await _context.Courses.CountAsync(c => c.IsActive);
            stats.TotalStudents = await _context.Students.CountAsync(s => s.IsActive);
            stats.TotalTeachers = await _context.Teachers.CountAsync(t => t.IsActive);

            // Get enrollment statistics
            var enrollments = await _context.Enrollments
                .Where(e => e.IsActive)
                .ToListAsync();

            stats.TotalEnrollments = enrollments.Count;
            stats.ActiveEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
            stats.CompletedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

            // Calculate completion rate
            if (stats.TotalEnrollments > 0)
            {
                stats.CompletionRate = Math.Round((decimal)stats.CompletedEnrollments / stats.TotalEnrollments * 100, 1);
            }

            // Calculate average grade
            var completedEnrollmentsWithGrades = enrollments
                .Where(e => e.Status == EnrollmentStatus.Completed && e.Grade.HasValue)
                .ToList();

            if (completedEnrollmentsWithGrades.Any())
            {
                stats.AverageGrade = Math.Round(completedEnrollmentsWithGrades.Average(e => e.Grade!.Value), 1);
            }

            // Get recent activities (last 10)
            stats.RecentActivities = await GetRecentActivitiesAsync();

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics");
            throw;
        }
    }

    private async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
    {
        try
        {
            var activities = new List<RecentActivityDto>();

            // Get recent enrollments (last 5)
            var recentEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var enrollment in recentEnrollments)
            {
                activities.Add(new RecentActivityDto
                {
                    Id = enrollment.Id.ToString(),
                    Type = "enrollment",
                    Description = $"{enrollment.Student?.User?.FullName} enrolled in {enrollment.Course?.Title}",
                    Timestamp = enrollment.CreatedAt,
                    StudentName = enrollment.Student?.User?.FullName,
                    CourseName = enrollment.Course?.Title
                });
            }

            // Get recent completions (last 5)
            var recentCompletions = await _context.Enrollments
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.IsActive && e.Status == EnrollmentStatus.Completed && e.CompletionDate.HasValue)
                .OrderByDescending(e => e.CompletionDate)
                .Take(5)
                .ToListAsync();

            foreach (var completion in recentCompletions)
            {
                activities.Add(new RecentActivityDto
                {
                    Id = completion.Id.ToString(),
                    Type = "completion",
                    Description = $"{completion.Student?.User?.FullName} completed {completion.Course?.Title}" + 
                                (completion.Grade.HasValue ? $" with grade {completion.Grade:F1}%" : ""),
                    Timestamp = completion.CompletionDate!.Value,
                    StudentName = completion.Student?.User?.FullName,
                    CourseName = completion.Course?.Title
                });
            }

            // Get recent courses (last 3)
            var recentCourses = await _context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .Take(3)
                .ToListAsync();

            foreach (var course in recentCourses)
            {
                activities.Add(new RecentActivityDto
                {
                    Id = course.Id.ToString(),
                    Type = "course_created",
                    Description = $"New course created: {course.Title} by {course.Instructor?.FullName ?? "Unknown"}",
                    Timestamp = course.CreatedAt,
                    CourseName = course.Title
                });
            }

            // Return sorted by timestamp, take most recent 10
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activities");
            return new List<RecentActivityDto>();
        }
    }
}