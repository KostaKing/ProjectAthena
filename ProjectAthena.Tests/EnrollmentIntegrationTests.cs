using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ProjectAthena.Dtos.Enrollments;
using ProjectAthena.Dtos.Common;
using ProjectAthena.Data.Models;
using Amazon.S3;
using Amazon.S3.Model;

namespace ProjectAthena.Tests;

[Collection("AspireApp")]
public class EnrollmentIntegrationTests 
{
    private readonly AspireAppFixture _fixture;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EnrollmentIntegrationTests(AspireAppFixture fixture)
    {
        _fixture = fixture;
        _httpClient = _fixture.App.CreateHttpClient("ProjectAthenaApi");
    }

    [Fact]
    public async Task GetAllEnrollments_ShouldReturnPagedResult()
    {
        _httpClient.AddTeacherAuth();
        var response = await _httpClient.GetAsync("/api/enrollments?page=1&pageSize=5");

        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<EnrollmentDto>>(content, _jsonOptions);

        Assert.NotNull(pagedResult);
        Assert.True(pagedResult.TotalCount >= 0);
        Assert.True(pagedResult.Page >= 1);
        Assert.True(pagedResult.PageSize > 0);
        Assert.NotNull(pagedResult.Items);
        Assert.True(pagedResult.Items.Count() <= pagedResult.PageSize);
    }

    [Fact]
    public async Task CreateEnrollment_ShouldCreateNewEnrollment_WhenValidData()
    {
        // Get actual seeded data from the database
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var studentsResponse = await _httpClient.GetWithAuthAsync("/api/students", adminToken);
        studentsResponse.EnsureSuccessStatusCode();
        var studentsJson = await studentsResponse.Content.ReadAsStringAsync();
        var studentsData = JsonDocument.Parse(studentsJson);
        var firstStudent = studentsData.RootElement.EnumerateArray().Skip(1).First(); // Use second student to avoid conflicts
        var studentId = firstStudent.GetProperty("id").GetString()!;
        
        // Clean up existing enrollments for this student first
        await CleanupStudentEnrollments(studentId, adminToken);
        
        // Use the CreateTestEnrollment method which handles conflicts automatically
        var enrollmentId = await CreateTestEnrollment(studentId);
        
        // Verify the enrollment was created successfully
        var getResponse = await _httpClient.GetWithAuthAsync($"/api/enrollments/{enrollmentId}", adminToken);
        getResponse.EnsureSuccessStatusCode();
        
        var content = await getResponse.Content.ReadAsStringAsync();
        var enrollment = JsonSerializer.Deserialize<EnrollmentDto>(content, _jsonOptions);

        Assert.NotNull(enrollment);
        Assert.Equal(studentId, enrollment.StudentId);
        Assert.NotEqual(Guid.Empty, enrollment.Id);
    }

    [Fact]
    public async Task GetEnrollmentsByStudent_ShouldReturnStudentEnrollments()
    {
        // Get a valid student ID from the database
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var studentsResponse = await _httpClient.GetWithAuthAsync("/api/students", adminToken);
        studentsResponse.EnsureSuccessStatusCode();
        var studentsJson = await studentsResponse.Content.ReadAsStringAsync();
        var studentsData = JsonDocument.Parse(studentsJson);
        var firstStudent = studentsData.RootElement.EnumerateArray().First();
        var studentId = firstStudent.GetProperty("id").GetString()!;
        
        var studentToken = TestAuthenticationHelper.GenerateStudentToken();
        var response = await _httpClient.GetWithAuthAsync($"/api/enrollments/student/{studentId}", studentToken);

        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var enrollments = JsonSerializer.Deserialize<IEnumerable<EnrollmentDto>>(content, _jsonOptions);

        Assert.NotNull(enrollments);
        Assert.All(enrollments, enrollment => 
            Assert.Equal(studentId, enrollment.StudentId));
    }

    [Fact]
    public async Task GetEnrollmentsByCourse_ShouldReturnCourseEnrollments()
    {
        // Get a valid course ID from the database
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var coursesResponse = await _httpClient.GetWithAuthAsync("/api/courses", adminToken);
        coursesResponse.EnsureSuccessStatusCode();
        var coursesJson = await coursesResponse.Content.ReadAsStringAsync();
        var coursesData = JsonDocument.Parse(coursesJson);
        var firstCourse = coursesData.RootElement.EnumerateArray().First();
        var courseId = Guid.Parse(firstCourse.GetProperty("id").GetString()!);
        
        var teacherToken = TestAuthenticationHelper.GenerateTeacherToken();
        var response = await _httpClient.GetWithAuthAsync($"/api/enrollments/course/{courseId}", teacherToken);

        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var enrollments = JsonSerializer.Deserialize<IEnumerable<EnrollmentDto>>(content, _jsonOptions);

        Assert.NotNull(enrollments);
        Assert.All(enrollments, enrollment => 
            Assert.Equal(courseId, enrollment.CourseId));
    }

    [Fact]
    public async Task UpdateEnrollmentStatus_ShouldUpdateEnrollment_WhenValidData()
    {
        // Get a valid student ID from the database
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var studentsResponse = await _httpClient.GetWithAuthAsync("/api/students", adminToken);
        studentsResponse.EnsureSuccessStatusCode();
        var studentsJson = await studentsResponse.Content.ReadAsStringAsync();
        var studentsData = JsonDocument.Parse(studentsJson);
        var students = studentsData.RootElement.EnumerateArray().ToList();
        var randomStudent = students[Random.Shared.Next(students.Count)];
        var studentId = randomStudent.GetProperty("id").GetString()!;
        
        var enrollmentId = await CreateTestEnrollment(studentId);
        
        var updateDto = new UpdateEnrollmentStatusDto(EnrollmentStatus.Completed, 95.5m);

        var teacherToken = TestAuthenticationHelper.GenerateTeacherToken();
        var response = await _httpClient.PatchAsJsonWithAuthAsync($"/api/enrollments/{enrollmentId}/status", updateDto, teacherToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var studentToken = TestAuthenticationHelper.GenerateStudentToken();
        var getResponse = await _httpClient.GetWithAuthAsync($"/api/enrollments/{enrollmentId}", studentToken);
        getResponse.EnsureSuccessStatusCode();
        
        var content = await getResponse.Content.ReadAsStringAsync();
        var enrollment = JsonSerializer.Deserialize<EnrollmentDto>(content, _jsonOptions);

        Assert.NotNull(enrollment);
        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);
        Assert.Equal(95.5m, enrollment.Grade);
    }

    [Fact]
    public async Task DeleteEnrollment_ShouldRemoveEnrollment_WhenExists()
    {
        // Get a valid student ID from the database
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var studentsResponse = await _httpClient.GetWithAuthAsync("/api/students", adminToken);
        studentsResponse.EnsureSuccessStatusCode();
        var studentsJson = await studentsResponse.Content.ReadAsStringAsync();
        var studentsData = JsonDocument.Parse(studentsJson);
        var students = studentsData.RootElement.EnumerateArray().ToList();
        var randomStudent = students[Random.Shared.Next(students.Count)];
        var studentId = randomStudent.GetProperty("id").GetString()!;
        
        var enrollmentId = await CreateTestEnrollment(studentId);

        var deleteResponse = await _httpClient.DeleteWithAuthAsync($"/api/enrollments/{enrollmentId}", adminToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var studentToken = TestAuthenticationHelper.GenerateStudentToken();
        var getResponse = await _httpClient.GetWithAuthAsync($"/api/enrollments/{enrollmentId}", studentToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task SaveEnrollmentDataToS3_ShouldStoreEnrollmentInformation_UsingAwsSdk()
    {
        var bucketName = "test-enrollment-bucket";
        var enrollmentKey = $"enrollments/enrollment-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        
        var mockS3Client = new MockS3Client();
        
        // Get a valid student ID from the database
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var studentsResponse = await _httpClient.GetWithAuthAsync("/api/students", adminToken);
        studentsResponse.EnsureSuccessStatusCode();
        var studentsJson = await studentsResponse.Content.ReadAsStringAsync();
        var studentsData = JsonDocument.Parse(studentsJson);
        var students = studentsData.RootElement.EnumerateArray().ToList();
        var randomStudent = students[Random.Shared.Next(students.Count)];
        var studentId = randomStudent.GetProperty("id").GetString()!;
        
        var enrollmentId = await CreateTestEnrollment(studentId);
        
        var response = await _httpClient.GetWithAuthAsync($"/api/enrollments/{enrollmentId}", adminToken);
        response.EnsureSuccessStatusCode();
        
        var enrollmentData = await response.Content.ReadAsStringAsync();
        
        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = enrollmentKey,
            ContentBody = enrollmentData,
            ContentType = "application/json",
            Metadata = 
            {
                ["enrollment-id"] = enrollmentId.ToString(),
                ["created-at"] = DateTime.UtcNow.ToString("O")
            }
        };

        var putResponse = await mockS3Client.PutObjectAsync(putRequest);
        
        Assert.NotNull(putResponse);
        Assert.Equal(System.Net.HttpStatusCode.OK, putResponse.HttpStatusCode);
        
        var getRequest = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = enrollmentKey
        };
        
        var getResponse = await mockS3Client.GetObjectAsync(getRequest);
        Assert.NotNull(getResponse);
        Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.HttpStatusCode);
        
        using var reader = new StreamReader(getResponse.ResponseStream);
        var retrievedData = await reader.ReadToEndAsync();
        
        Assert.Equal(enrollmentData, retrievedData);
        
        var retrievedEnrollment = JsonSerializer.Deserialize<EnrollmentDto>(retrievedData, _jsonOptions);
        Assert.NotNull(retrievedEnrollment);
        Assert.Equal(enrollmentId, retrievedEnrollment.Id);
    }

    [Fact]
    public async Task BulkEnrollmentOperationsWithS3Backup_ShouldProcessMultipleEnrollments()
    {
        var bucketName = "test-bulk-enrollment-bucket";
        var mockS3Client = new MockS3Client();
        
        var enrollmentIds = new List<Guid>();
        
        // Get actual student IDs from the database
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var studentsResponse = await _httpClient.GetWithAuthAsync("/api/students", adminToken);
        studentsResponse.EnsureSuccessStatusCode();
        var studentsJson = await studentsResponse.Content.ReadAsStringAsync();
        var studentsData = JsonDocument.Parse(studentsJson);
        var students = studentsData.RootElement.EnumerateArray().Take(3).ToList();
        
        // Clean up existing enrollments for these students first
        for (int i = 0; i < students.Count; i++)
        {
            var studentId = students[i].GetProperty("id").GetString()!;
            await CleanupStudentEnrollments(studentId, adminToken);
        }
        
        for (int i = 0; i < students.Count; i++)
        {
            var studentId = students[i].GetProperty("id").GetString()!;
            var enrollmentId = await CreateTestEnrollment(studentId);
            enrollmentIds.Add(enrollmentId);
        }
        
        var enrollmentBackups = new List<string>();
        
        foreach (var enrollmentId in enrollmentIds)
        {
            var response = await _httpClient.GetWithAuthAsync($"/api/enrollments/{enrollmentId}", adminToken);
            response.EnsureSuccessStatusCode();
            
            var enrollmentData = await response.Content.ReadAsStringAsync();
            enrollmentBackups.Add(enrollmentData);
            
            var backupKey = $"bulk-enrollments/enrollment-{enrollmentId}.json";
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = backupKey,
                ContentBody = enrollmentData,
                ContentType = "application/json"
            };

            var putResponse = await mockS3Client.PutObjectAsync(putRequest);
            Assert.Equal(System.Net.HttpStatusCode.OK, putResponse.HttpStatusCode);
        }
        
        Assert.Equal(3, enrollmentBackups.Count);
        
        foreach (var enrollmentId in enrollmentIds)
        {
            var backupKey = $"bulk-enrollments/enrollment-{enrollmentId}.json";
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = backupKey
            };
            
            var getResponse = await mockS3Client.GetObjectAsync(getRequest);
            Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.HttpStatusCode);
        }
    }

    private async Task<Guid> CreateTestEnrollment(string studentId)
    {
        // Get a valid course ID from the database - try different courses until we find one that works
        var adminToken = TestAuthenticationHelper.GenerateAdminToken();
        var coursesResponse = await _httpClient.GetWithAuthAsync("/api/courses", adminToken);
        coursesResponse.EnsureSuccessStatusCode();
        var coursesJson = await coursesResponse.Content.ReadAsStringAsync();
        var coursesData = JsonDocument.Parse(coursesJson);
        var courses = coursesData.RootElement.EnumerateArray().ToList();
        
        // First, try to get existing enrollments for this student to avoid duplicates
        var enrollmentsResponse = await _httpClient.GetWithAuthAsync($"/api/enrollments/student/{studentId}", adminToken);
        var existingEnrollments = new HashSet<Guid>();
        if (enrollmentsResponse.IsSuccessStatusCode)
        {
            var enrollmentsJson = await enrollmentsResponse.Content.ReadAsStringAsync();
            var enrollmentsData = JsonDocument.Parse(enrollmentsJson);
            foreach (var enrollment in enrollmentsData.RootElement.EnumerateArray())
            {
                var courseIdProp = enrollment.GetProperty("courseId").GetString();
                if (Guid.TryParse(courseIdProp, out var courseId))
                {
                    existingEnrollments.Add(courseId);
                }
            }
        }
        
        // Try different courses until we find one where the student isn't already enrolled
        foreach (var course in courses)
        {
            var courseId = Guid.Parse(course.GetProperty("id").GetString()!);
            
            // Skip if student is already enrolled in this course
            if (existingEnrollments.Contains(courseId))
                continue;
                
            var createEnrollmentDto = new CreateEnrollmentDto
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonWithAuthAsync("/api/enrollments", createEnrollmentDto, adminToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var enrollment = JsonSerializer.Deserialize<EnrollmentDto>(content, _jsonOptions);
                return enrollment!.Id;
            }
            // If this course didn't work for other reasons (capacity, etc.), try the next one
        }
        
        throw new InvalidOperationException($"Could not create enrollment for student {studentId} - no available courses found (checked {courses.Count} courses, student already enrolled in {existingEnrollments.Count})");
    }

    private async Task CleanupStudentEnrollments(string studentId, string adminToken)
    {
        try
        {
            // Get all enrollments for this student
            var enrollmentsResponse = await _httpClient.GetWithAuthAsync($"/api/enrollments/student/{studentId}", adminToken);
            if (!enrollmentsResponse.IsSuccessStatusCode)
                return; // No enrollments found, which is fine
                
            var enrollmentsJson = await enrollmentsResponse.Content.ReadAsStringAsync();
            var enrollmentsData = JsonDocument.Parse(enrollmentsJson);
            
            // Delete each enrollment
            foreach (var enrollment in enrollmentsData.RootElement.EnumerateArray())
            {
                var enrollmentId = enrollment.GetProperty("id").GetString();
                if (!string.IsNullOrEmpty(enrollmentId))
                {
                    await _httpClient.DeleteWithAuthAsync($"/api/enrollments/{enrollmentId}", adminToken);
                    // Ignore errors - the enrollment might already be deleted
                }
            }
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
    }
}

public record UpdateEnrollmentStatusDto(EnrollmentStatus Status, decimal? Grade);