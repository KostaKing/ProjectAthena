using Bogus;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Models.Students;
using ProjectAthena.Data.Persistence;

namespace ProjectAthena.DbWorkerService.Seeders;

public class EnrollmentSeeder : ISeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly SeedingConfiguration _config;
    private readonly ILogger<EnrollmentSeeder> _logger;

    public EnrollmentSeeder(
        ApplicationDbContext context,
        SeedingConfiguration config,
        ILogger<EnrollmentSeeder> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Enrollments.AnyAsync())
        {
            _logger.LogInformation("Enrollments already exist, skipping seeding");
            return;
        }

        var students = await _context.Students
            .Include(s => s.User)
            .Where(s => s.IsActive)
            .ToListAsync();

        var courses = await _context.Courses
            .Where(c => c.IsActive)
            .ToListAsync();

        if (!students.Any() || !courses.Any())
        {
            _logger.LogWarning("No students or courses found, cannot seed enrollments");
            return;
        }

        var enrollments = new List<Enrollment>();
        var enrollmentFaker = new Faker();

        foreach (var student in students)
        {
            var enrollmentCount = enrollmentFaker.Random.Number(
                _config.MinEnrollmentsPerStudent, 
                _config.MaxEnrollmentsPerStudent);

            var availableCourses = courses.ToList();
            
            for (int i = 0; i < enrollmentCount && availableCourses.Any(); i++)
            {
                var selectedCourse = enrollmentFaker.PickRandom(availableCourses);
                availableCourses.Remove(selectedCourse); // Prevent duplicate enrollments

                var enrollmentDate = enrollmentFaker.Date.PastOffset(1).UtcDateTime;
                var status = GenerateEnrollmentStatus(enrollmentFaker, enrollmentDate);
                
                var enrollment = new Enrollment
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    CourseId = selectedCourse.Id,
                    EnrollmentDate = enrollmentDate,
                    Status = status,
                    Grade = GenerateGrade(enrollmentFaker, status),
                    CompletionDate = GenerateCompletionDate(enrollmentFaker, status, enrollmentDate),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                enrollments.Add(enrollment);
            }
        }

        await _context.Enrollments.AddRangeAsync(enrollments);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} enrollments for {StudentCount} students", 
            enrollments.Count, students.Count);
    }

    private EnrollmentStatus GenerateEnrollmentStatus(Faker faker, DateTime enrollmentDate)
    {
        var daysSinceEnrollment = (DateTime.UtcNow - enrollmentDate).Days;
        
        // More realistic status distribution based on time
        if (daysSinceEnrollment < 30)
        {
            // Recent enrollments are mostly active
            return faker.Random.WeightedRandom(
                new[] { EnrollmentStatus.Active, EnrollmentStatus.Dropped },
                new[] { 0.85f, 0.15f });
        }
        else if (daysSinceEnrollment < 120)
        {
            // Mid-term enrollments
            return faker.Random.WeightedRandom(
                new[] { EnrollmentStatus.Active, EnrollmentStatus.Completed, EnrollmentStatus.Dropped },
                new[] { 0.60f, 0.25f, 0.15f });
        }
        else
        {
            // Older enrollments are mostly completed or dropped
            return faker.Random.WeightedRandom(
                new[] { EnrollmentStatus.Completed, EnrollmentStatus.Dropped, EnrollmentStatus.Active },
                new[] { 0.70f, 0.20f, 0.10f });
        }
    }

    private decimal? GenerateGrade(Faker faker, EnrollmentStatus status)
    {
        return status switch
        {
            EnrollmentStatus.Completed => Math.Round((decimal)faker.Random.Double(60, 100), 2),
            EnrollmentStatus.Active => faker.Random.Bool(0.3f) ? Math.Round((decimal)faker.Random.Double(70, 95), 2) : null,
            _ => null
        };
    }

    private DateTime? GenerateCompletionDate(Faker faker, EnrollmentStatus status, DateTime enrollmentDate)
    {
        if (status == EnrollmentStatus.Completed)
        {
            return faker.Date.BetweenOffset(
                enrollmentDate.AddDays(30), 
                enrollmentDate.AddDays(120)).UtcDateTime;
        }
        
        return null;
    }
}