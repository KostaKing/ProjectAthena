namespace ProjectAthena.DbWorkerService.Seeders;

public class SeedingConfiguration
{
    public int AdminCount { get; set; } = 2;
    public int TeacherCount { get; set; } = 10;
    public int StudentCount { get; set; } = 50;
    public int CourseCount { get; set; } = 15;
    public int MaxEnrollmentsPerStudent { get; set; } = 5;
    public int MinEnrollmentsPerStudent { get; set; } = 2;
    public string DefaultPassword { get; set; } = "Admin123!";
}