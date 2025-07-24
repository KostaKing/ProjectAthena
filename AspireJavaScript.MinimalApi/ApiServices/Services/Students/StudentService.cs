using AspireJavaScript.MinimalApi.ApiServices.Interfaces.Students;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models.Students;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Dtos.Mappings;
using ProjectAthena.Dtos.Students;

namespace AspireJavaScript.MinimalApi.ApiServices.Services.Students;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(ApplicationDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        try
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .OrderBy(s => s.StudentNumber)
                .ToListAsync();

            return students.Select(s => s.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all students");
            throw;
        }
    }

    public async Task<StudentDto?> GetStudentByIdAsync(Guid id)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            return student?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with ID: {StudentId}", id);
            throw;
        }
    }

    public async Task<StudentDto?> GetStudentByUserIdAsync(string userId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            return student?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<StudentDto?> GetStudentByStudentNumberAsync(string studentNumber)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber && s.IsActive);

            return student?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with student number: {StudentNumber}", studentNumber);
            throw;
        }
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        try
        {
            if (await StudentNumberExistsAsync(createStudentDto.StudentNumber))
            {
                throw new InvalidOperationException($"Student with number '{createStudentDto.StudentNumber}' already exists.");
            }

            var student = createStudentDto.ToEntity();
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return await GetStudentByIdAsync(student.Id) ?? throw new InvalidOperationException("Failed to retrieve created student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student with number: {StudentNumber}", createStudentDto.StudentNumber);
            throw;
        }
    }

    public async Task<StudentDto?> UpdateStudentAsync(Guid id, UpdateStudentDto updateStudentDto)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (student == null)
            {
                return null;
            }

            updateStudentDto.UpdateEntity(student);
            await _context.SaveChangesAsync();

            return await GetStudentByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student with ID: {StudentId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteStudentAsync(Guid id)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (student == null)
            {
                return false;
            }

            student.IsActive = false;
            student.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student with ID: {StudentId}", id);
            throw;
        }
    }

    public async Task<bool> StudentNumberExistsAsync(string studentNumber)
    {
        try
        {
            return await _context.Students
                .AnyAsync(s => s.StudentNumber == studentNumber && s.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if student number exists: {StudentNumber}", studentNumber);
            throw;
        }
    }
}