using ProjectAthena.Dtos.Enrollments;
using ProjectAthena.Data.Models;

namespace AspireJavaScript.MinimalApi.ApiServices.Interfaces;

public interface IEnrollmentService
{
    Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync();
    Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudentIdAsync(string studentId);
    Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseIdAsync(Guid courseId);
    Task<EnrollmentDto?> GetEnrollmentByIdAsync(Guid id);
    Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<bool> UpdateEnrollmentStatusAsync(Guid id, EnrollmentStatus status, decimal? grade = null);
    Task<bool> DeleteEnrollmentAsync(Guid id);
    Task<EnrollmentReportDto?> GenerateEnrollmentReportAsync(Guid courseId);
    Task<bool> IsStudentEnrolledAsync(string studentId, Guid courseId);
}