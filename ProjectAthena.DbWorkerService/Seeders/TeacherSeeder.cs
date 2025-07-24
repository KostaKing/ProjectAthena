using Bogus;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Models.Teachers;
using ProjectAthena.Data.Persistence;

namespace ProjectAthena.DbWorkerService.Seeders;

public class TeacherSeeder : ISeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly SeedingConfiguration _config;
    private readonly ILogger<TeacherSeeder> _logger;

    private readonly string[] _departments = {
        "Computer Science", "Mathematics", "Physics", "Chemistry", "Biology",
        "English Literature", "History", "Psychology", "Economics", "Engineering",
        "Business Administration", "Art", "Music", "Philosophy", "Political Science"
    };

    private readonly string[] _titles = {
        "Professor", "Associate Professor", "Assistant Professor", "Senior Lecturer",
        "Lecturer", "Instructor", "Adjunct Professor"
    };

    private readonly string[] _qualifications = {
        "Ph.D. in Computer Science", "Ph.D. in Mathematics", "Ph.D. in Physics",
        "Ph.D. in Chemistry", "Ph.D. in Biology", "Ph.D. in English Literature",
        "Ph.D. in History", "Ph.D. in Psychology", "Ph.D. in Economics",
        "M.S. in Engineering", "M.B.A.", "M.F.A.", "Ph.D. in Philosophy",
        "Ph.D. in Political Science", "M.Ed."
    };

    public TeacherSeeder(
        ApplicationDbContext context,
        SeedingConfiguration config,
        ILogger<TeacherSeeder> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Teachers.AnyAsync())
        {
            _logger.LogInformation("Teachers already exist, skipping seeding");
            return;
        }

        var teacherUsers = await _context.Users
            .Where(u => u.Role == UserRole.Teacher)
            .ToListAsync();

        if (!teacherUsers.Any())
        {
            _logger.LogWarning("No teacher users found, cannot seed teachers");
            return;
        }

        var teacherFaker = new Faker<Teacher>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.UserId, f => f.PickRandom(teacherUsers).Id)
            .RuleFor(t => t.EmployeeNumber, f => f.Random.Replace("EMP####"))
            .RuleFor(t => t.Department, f => f.PickRandom(_departments))
            .RuleFor(t => t.Title, f => f.PickRandom(_titles))
            .RuleFor(t => t.Qualifications, f => f.PickRandom(_qualifications))
            .RuleFor(t => t.Specialization, (f, t) => GenerateSpecialization(f, t.Department))
            .RuleFor(t => t.Phone, f => f.Phone.PhoneNumber("###-###-####"))
            .RuleFor(t => t.OfficeLocation, f => $"{f.Random.AlphaNumeric(1).ToUpper()}{f.Random.Number(100, 999)}")
            .RuleFor(t => t.HireDate, f => f.Date.PastOffset(10).UtcDateTime)
            .RuleFor(t => t.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime)
            .RuleFor(t => t.IsActive, f => true);

        var teachers = new List<Teacher>();
        var usedUserIds = new HashSet<string>();

        // Generate teachers ensuring each user gets only one teacher record
        foreach (var user in teacherUsers)
        {
            if (usedUserIds.Contains(user.Id)) continue;

            var teacher = teacherFaker.Generate();
            teacher.UserId = user.Id;
            teacher.EmployeeNumber = GenerateUniqueEmployeeNumber(teachers);
            
            teachers.Add(teacher);
            usedUserIds.Add(user.Id);
        }

        await _context.Teachers.AddRangeAsync(teachers);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} teachers", teachers.Count);
    }

    private string GenerateUniqueEmployeeNumber(List<Teacher> existingTeachers)
    {
        string employeeNumber;
        var faker = new Faker();
        
        do
        {
            employeeNumber = faker.Random.Replace("EMP####");
        }
        while (existingTeachers.Any(t => t.EmployeeNumber == employeeNumber));

        return employeeNumber;
    }

    private string GenerateSpecialization(Faker faker, string? department)
    {
        var specializations = department switch
        {
            "Computer Science" => new[] { "Artificial Intelligence", "Machine Learning", "Cybersecurity", "Software Engineering", "Data Science", "Web Development" },
            "Mathematics" => new[] { "Algebra", "Calculus", "Statistics", "Geometry", "Number Theory", "Applied Mathematics" },
            "Physics" => new[] { "Quantum Physics", "Astrophysics", "Thermodynamics", "Electromagnetism", "Nuclear Physics" },
            "Chemistry" => new[] { "Organic Chemistry", "Inorganic Chemistry", "Physical Chemistry", "Biochemistry" },
            "Biology" => new[] { "Molecular Biology", "Genetics", "Ecology", "Microbiology", "Botany", "Zoology" },
            "English Literature" => new[] { "Victorian Literature", "Modern Literature", "Poetry", "Creative Writing", "Comparative Literature" },
            "History" => new[] { "Ancient History", "Medieval History", "Modern History", "American History", "European History" },
            "Psychology" => new[] { "Clinical Psychology", "Cognitive Psychology", "Social Psychology", "Developmental Psychology" },
            "Economics" => new[] { "Macroeconomics", "Microeconomics", "International Economics", "Labor Economics" },
            "Engineering" => new[] { "Mechanical Engineering", "Electrical Engineering", "Civil Engineering", "Chemical Engineering" },
            _ => new[] { "General Studies", "Interdisciplinary Studies", "Research Methods" }
        };

        return faker.PickRandom(specializations);
    }
}