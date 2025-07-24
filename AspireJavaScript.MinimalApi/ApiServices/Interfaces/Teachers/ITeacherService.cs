using ProjectAthena.Dtos.Teachers;

namespace AspireJavaScript.MinimalApi.ApiServices.Interfaces.Teachers;

public interface ITeacherService
{
    Task<IEnumerable<TeacherDto>> GetAllTeachersAsync();
    Task<TeacherDto?> GetTeacherByIdAsync(Guid id);
    Task<TeacherDto?> GetTeacherByUserIdAsync(string userId);
    Task<TeacherDto?> GetTeacherByEmployeeNumberAsync(string employeeNumber);
    Task<TeacherDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto);
    Task<TeacherDto?> UpdateTeacherAsync(Guid id, UpdateTeacherDto updateTeacherDto);
    Task<bool> DeleteTeacherAsync(Guid id);
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber);
}