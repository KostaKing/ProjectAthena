using ProjectAthena.Dtos.Students;

namespace AspireJavaScript.MinimalApi.ApiServices.Interfaces.Students;

public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
    Task<StudentDto?> GetStudentByIdAsync(Guid id);
    Task<StudentDto?> GetStudentByUserIdAsync(string userId);
    Task<StudentDto?> GetStudentByStudentNumberAsync(string studentNumber);
    Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
    Task<StudentDto?> UpdateStudentAsync(Guid id, UpdateStudentDto updateStudentDto);
    Task<bool> DeleteStudentAsync(Guid id);
    Task<bool> StudentNumberExistsAsync(string studentNumber);
}