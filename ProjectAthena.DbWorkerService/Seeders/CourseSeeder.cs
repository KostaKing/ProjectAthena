using Bogus;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Models.Teachers;
using ProjectAthena.Data.Persistence;

namespace ProjectAthena.DbWorkerService.Seeders;

public class CourseSeeder : ISeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly SeedingConfiguration _config;
    private readonly ILogger<CourseSeeder> _logger;

    private readonly (string Code, string Title, string Description, int Credits)[] _courseTemplates = {
        ("CS101", "Introduction to Computer Science", "Basic principles of computer science and programming fundamentals", 3),
        ("CS201", "Data Structures and Algorithms", "Advanced data structures and algorithmic problem solving", 4),
        ("CS301", "Software Engineering", "Software development lifecycle and engineering practices", 3),
        ("CS401", "Database Systems", "Database design, implementation, and management", 3),
        ("MATH101", "Calculus I", "Differential and integral calculus of single variable functions", 4),
        ("MATH201", "Linear Algebra", "Vector spaces, matrices, and linear transformations", 3),
        ("MATH301", "Statistics", "Probability theory and statistical inference", 3),
        ("PHYS101", "General Physics I", "Mechanics, thermodynamics, and wave motion", 4),
        ("PHYS201", "General Physics II", "Electricity, magnetism, and optics", 4),
        ("CHEM101", "General Chemistry", "Atomic structure, bonding, and chemical reactions", 4),
        ("BIOL101", "General Biology", "Cell biology, genetics, and evolution", 4),
        ("ENG101", "English Composition", "Academic writing and critical thinking skills", 3),
        ("HIST101", "World History", "Survey of world civilizations and cultures", 3),
        ("PSYC101", "Introduction to Psychology", "Basic principles of human behavior and mental processes", 3),
        ("ECON101", "Principles of Economics", "Microeconomic and macroeconomic fundamentals", 3),
        ("ART101", "Introduction to Art", "Art history and appreciation across cultures", 3),
        ("MUS101", "Music Theory", "Basic music theory and composition", 3),
        ("PHIL101", "Introduction to Philosophy", "Major philosophical questions and thinkers", 3),
        ("POLI101", "Political Science", "Government systems and political theory", 3),
        ("BUS101", "Business Administration", "Introduction to business principles and management", 3)
    };

    public CourseSeeder(
        ApplicationDbContext context,
        SeedingConfiguration config,
        ILogger<CourseSeeder> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Courses.AnyAsync())
        {
            _logger.LogInformation("Courses already exist, skipping seeding");
            return;
        }

        var teachers = await _context.Teachers
            .Include(t => t.User)
            .Where(t => t.IsActive)
            .ToListAsync();

        if (!teachers.Any())
        {
            _logger.LogWarning("No teachers found, creating courses without instructors");
        }

        var coursesToCreate = Math.Min(_config.CourseCount, _courseTemplates.Length);
        var selectedTemplates = _courseTemplates.Take(coursesToCreate).ToArray();

        var courseFaker = new Faker<Course>()
            .RuleFor(c => c.Id, f => Guid.NewGuid())
            .RuleFor(c => c.Title, f => f.PickRandom(selectedTemplates).Title)
            .RuleFor(c => c.Description, f => f.PickRandom(selectedTemplates).Description)
            .RuleFor(c => c.CourseCode, f => f.PickRandom(selectedTemplates).Code)
            .RuleFor(c => c.Credits, f => f.PickRandom(selectedTemplates).Credits)
            .RuleFor(c => c.InstructorId, f => teachers.Any() ? f.PickRandom(teachers).UserId : null)
            .RuleFor(c => c.StartDate, f => f.Date.Future(1, DateTime.UtcNow.AddMonths(1)))
            .RuleFor(c => c.MaxEnrollments, f => f.Random.Number(20, 100))
            .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime)
            .RuleFor(c => c.IsActive, f => true);

        var courses = new List<Course>();
        var usedCourseCodes = new HashSet<string>();

        // Generate courses ensuring unique course codes
        foreach (var template in selectedTemplates)
        {
            if (usedCourseCodes.Contains(template.Code)) continue;

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Title = template.Title,
                Description = template.Description,
                CourseCode = template.Code,
                Credits = template.Credits,
                InstructorId = teachers.Any() ? new Faker().PickRandom(teachers).UserId : null,
                StartDate = new Faker().Date.Future(1, DateTime.UtcNow.AddMonths(1)),
                EndDate = new Faker().Date.Future(1, DateTime.UtcNow.AddMonths(4)),
                MaxEnrollments = new Faker().Random.Number(20, 100),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Ensure end date is after start date
            course.EndDate = course.StartDate.AddMonths(3).AddDays(new Faker().Random.Number(0, 30));

            courses.Add(course);
            usedCourseCodes.Add(template.Code);
        }

        await _context.Courses.AddRangeAsync(courses);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} courses", courses.Count);
    }
}