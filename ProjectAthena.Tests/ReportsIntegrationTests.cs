using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ProjectAthena.Dtos.Enrollments;
using Amazon.S3;
using Amazon.S3.Model;

namespace ProjectAthena.Tests;

[Collection("AspireApp")]
public class ReportsIntegrationTests
{
    private readonly AspireAppFixture _fixture;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ReportsIntegrationTests(AspireAppFixture fixture)
    {
        _fixture = fixture;
        _httpClient = _fixture.App.CreateHttpClient("ProjectAthenaApi");
    }

    [Fact]
    public async Task GenerateEnrollmentReport_ShouldReturnValidReport_WhenCourseExists()
    {
        var teacherToken = TestAuthenticationHelper.GenerateTeacherToken();
        
        // Get a valid course ID from the database
        var coursesResponse = await _httpClient.GetWithAuthAsync("/api/courses", teacherToken);
        coursesResponse.EnsureSuccessStatusCode();
        var coursesJson = await coursesResponse.Content.ReadAsStringAsync();
        var coursesData = JsonDocument.Parse(coursesJson);
        var firstCourse = coursesData.RootElement.EnumerateArray().First();
        var courseId = Guid.Parse(firstCourse.GetProperty("id").GetString()!);

        var response = await _httpClient.GetWithAuthAsync($"/api/enrollments/report/{courseId}", teacherToken);

        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var report = JsonSerializer.Deserialize<EnrollmentReportDto>(content, _jsonOptions);

        Assert.NotNull(report);
        Assert.NotNull(report.CourseTitle);
        Assert.True(report.TotalEnrollments >= 0);
        Assert.NotNull(report.StudentEnrollments);
        Assert.True(report.StudentEnrollments.Count >= 0);
    }

    [Fact]
    public async Task GenerateAdvancedEnrollmentReport_ShouldReturnValidReport_WithFilters()
    {
        var teacherToken = TestAuthenticationHelper.GenerateTeacherToken();
        var reportRequest = new EnrollmentReportRequestDto
        {
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow,
            GroupBy = ReportGroupBy.Course
        };

        var response = await _httpClient.PostAsJsonWithAuthAsync("/api/enrollments/reports/advanced", reportRequest, teacherToken);

        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var report = JsonSerializer.Deserialize<EnrollmentReportResponseDto>(content, _jsonOptions);

        Assert.NotNull(report);
        Assert.NotNull(report.Items);
        Assert.True(report.Summary.TotalEnrollments >= 0);
        Assert.NotNull(report.GeneratedAt);
        Assert.True(report.GeneratedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task SaveReportToS3_ShouldStoreReportData_UsingAwsSdk()
    {
        var bucketName = "test-reports-bucket";
        var reportKey = $"enrollment-reports/report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        
        var mockS3Client = new MockS3Client();
        
        var reportRequest = new EnrollmentReportRequestDto
        {
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            GroupBy = ReportGroupBy.Status
        };

        var teacherToken = TestAuthenticationHelper.GenerateTeacherToken();
        var response = await _httpClient.PostAsJsonWithAuthAsync("/api/enrollments/reports/advanced", reportRequest, teacherToken);
        response.EnsureSuccessStatusCode();
        
        var reportContent = await response.Content.ReadAsStringAsync();
        
        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = reportKey,
            ContentBody = reportContent,
            ContentType = "application/json"
        };

        var putResponse = await mockS3Client.PutObjectAsync(putRequest);
        
        Assert.NotNull(putResponse);
        Assert.Equal(System.Net.HttpStatusCode.OK, putResponse.HttpStatusCode);
        Assert.NotNull(putResponse.ETag);
        
        var getRequest = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = reportKey
        };
        
        var getResponse = await mockS3Client.GetObjectAsync(getRequest);
        Assert.NotNull(getResponse);
        Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.HttpStatusCode);
        
        using var reader = new StreamReader(getResponse.ResponseStream);
        var retrievedContent = await reader.ReadToEndAsync();
        
        Assert.Equal(reportContent, retrievedContent);
    }
}

