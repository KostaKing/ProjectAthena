namespace ProjectAthena.Dtos.Dashboard;

public class DashboardStatsDto
{
    public int TotalCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal? AverageGrade { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "enrollment", "completion", "course_created"
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? StudentName { get; set; }
    public string? CourseName { get; set; }
}