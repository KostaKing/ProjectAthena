using AspireJavaScript.MinimalApi.ApiServices.Interfaces.Teachers;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models.Teachers;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Dtos.Mappings;
using ProjectAthena.Dtos.Teachers;

namespace AspireJavaScript.MinimalApi.ApiServices.Services.Teachers;

public class TeacherService : ITeacherService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(ApplicationDbContext context, ILogger<TeacherService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TeacherDto>> GetAllTeachersAsync()
    {
        try
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Where(t => t.IsActive)
                .OrderBy(t => t.EmployeeNumber)
                .ToListAsync();

            return teachers.Select(t => t.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all teachers");
            throw;
        }
    }

    public async Task<TeacherDto?> GetTeacherByIdAsync(Guid id)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            return teacher?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher with ID: {TeacherId}", id);
            throw;
        }
    }

    public async Task<TeacherDto?> GetTeacherByUserIdAsync(string userId)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId && t.IsActive);

            return teacher?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher with UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<TeacherDto?> GetTeacherByEmployeeNumberAsync(string employeeNumber)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.EmployeeNumber == employeeNumber && t.IsActive);

            return teacher?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher with employee number: {EmployeeNumber}", employeeNumber);
            throw;
        }
    }

    public async Task<TeacherDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto)
    {
        try
        {
            if (await EmployeeNumberExistsAsync(createTeacherDto.EmployeeNumber))
            {
                throw new InvalidOperationException($"Teacher with employee number '{createTeacherDto.EmployeeNumber}' already exists.");
            }

            var teacher = createTeacherDto.ToEntity();
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return await GetTeacherByIdAsync(teacher.Id) ?? throw new InvalidOperationException("Failed to retrieve created teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher with employee number: {EmployeeNumber}", createTeacherDto.EmployeeNumber);
            throw;
        }
    }

    public async Task<TeacherDto?> UpdateTeacherAsync(Guid id, UpdateTeacherDto updateTeacherDto)
    {
        try
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (teacher == null)
            {
                return null;
            }

            updateTeacherDto.UpdateEntity(teacher);
            await _context.SaveChangesAsync();

            return await GetTeacherByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher with ID: {TeacherId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTeacherAsync(Guid id)
    {
        try
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (teacher == null)
            {
                return false;
            }

            teacher.IsActive = false;
            teacher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher with ID: {TeacherId}", id);
            throw;
        }
    }

    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber)
    {
        try
        {
            return await _context.Teachers
                .AnyAsync(t => t.EmployeeNumber == employeeNumber && t.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if employee number exists: {EmployeeNumber}", employeeNumber);
            throw;
        }
    }
}