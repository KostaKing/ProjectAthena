using Bogus;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Models.Students;
using ProjectAthena.Data.Persistence;

namespace ProjectAthena.DbWorkerService.Seeders;

public class StudentSeeder : ISeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly SeedingConfiguration _config;
    private readonly ILogger<StudentSeeder> _logger;

    public StudentSeeder(
        ApplicationDbContext context,
        SeedingConfiguration config,
        ILogger<StudentSeeder> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Students.AnyAsync())
        {
            _logger.LogInformation("Students already exist, skipping seeding");
            return;
        }

        var studentUsers = await _context.Users
            .Where(u => u.Role == UserRole.Student)
            .ToListAsync();

        if (!studentUsers.Any())
        {
            _logger.LogWarning("No student users found, cannot seed students");
            return;
        }

        var studentFaker = new Faker<Student>()
            .RuleFor(s => s.Id, f => Guid.NewGuid())
            .RuleFor(s => s.UserId, f => f.PickRandom(studentUsers).Id)
            .RuleFor(s => s.StudentNumber, f => f.Random.Replace("STU######"))
            .RuleFor(s => s.DateOfBirth, f => f.Date.PastOffset(25, DateTime.UtcNow.AddYears(-18)).UtcDateTime)
            .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber("###-###-####"))
            .RuleFor(s => s.Address, f => f.Address.FullAddress())
            .RuleFor(s => s.EmergencyContact, f => f.Name.FullName())
            .RuleFor(s => s.EmergencyContactPhone, f => f.Phone.PhoneNumber("###-###-####"))
            .RuleFor(s => s.EnrollmentDate, f => f.Date.PastOffset(2).UtcDateTime)
            .RuleFor(s => s.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime)
            .RuleFor(s => s.IsActive, f => true);

        var students = new List<Student>();
        var usedUserIds = new HashSet<string>();

        // Generate students ensuring each user gets only one student record
        foreach (var user in studentUsers)
        {
            if (usedUserIds.Contains(user.Id)) continue;

            var student = studentFaker.Generate();
            student.UserId = user.Id;
            student.StudentNumber = GenerateUniqueStudentNumber(students);
            
            students.Add(student);
            usedUserIds.Add(user.Id);
        }

        await _context.Students.AddRangeAsync(students);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} students", students.Count);
    }

    private string GenerateUniqueStudentNumber(List<Student> existingStudents)
    {
        string studentNumber;
        var faker = new Faker();
        
        do
        {
            studentNumber = faker.Random.Replace("STU######");
        }
        while (existingStudents.Any(s => s.StudentNumber == studentNumber));

        return studentNumber;
    }
}