using ProjectAthena.Dtos.Enrollments;
using ProjectAthena.Dtos.Common;
using ProjectAthena.Data.Models;

namespace AspireJavaScript.MinimalApi.ApiServices.Interfaces;

public interface IEnrollmentService
{
    Task<PagedResult<EnrollmentDto>> GetAllEnrollmentsAsync(string? search = null, int? status = null, int page = 1, int pageSize = 10);
    Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudentIdAsync(string studentId);
    Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseIdAsync(Guid courseId);
    Task<EnrollmentDto?> GetEnrollmentByIdAsync(Guid id);
    Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<bool> UpdateEnrollmentStatusAsync(Guid id, EnrollmentStatus status, decimal? grade = null);
    Task<bool> DeleteEnrollmentAsync(Guid id);
    Task<EnrollmentReportDto?> GenerateEnrollmentReportAsync(Guid courseId);
    Task<EnrollmentReportResponseDto> GenerateAdvancedEnrollmentReportAsync(EnrollmentReportRequestDto request);
    Task<bool> IsStudentEnrolledAsync(string studentId, Guid courseId);
    Task<bool> HasStudentEverEnrolledAsync(string studentId, Guid courseId);
}