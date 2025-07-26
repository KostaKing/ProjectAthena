using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectAthena.Dtos.Enrollments;
using Amazon.S3;

namespace ProjectAthena.Tests;

[Collection("AspireApp")]
public class AwsS3IntegrationTests : IDisposable
{
    private readonly AspireAppFixture _fixture;
    private readonly IAwsS3IntegrationService _s3Service;
    private readonly IServiceScope _scope;

    public AwsS3IntegrationTests(AspireAppFixture fixture)
    {
        _fixture = fixture;
        
        // Create a temporary service collection for S3 services
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<MockS3Client>();
        services.AddScoped<IAwsS3IntegrationService, AwsS3IntegrationService>();
        var serviceProvider = services.BuildServiceProvider();
        
        _scope = serviceProvider.CreateScope();
        _s3Service = _scope.ServiceProvider.GetRequiredService<IAwsS3IntegrationService>();
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }

    [Fact]
    public async Task CreateBucket_ShouldCreateBucketSuccessfully()
    {
        var bucketName = "test-integration-bucket";

        var result = await _s3Service!.CreateBucketAsync(bucketName);

        Assert.True(result);
        
        var bucketExists = await _s3Service.BucketExistsAsync(bucketName);
        Assert.True(bucketExists);
    }

    [Fact]
    public async Task SaveAndRetrieveReport_ShouldWorkEndToEnd()
    {
        var bucketName = "test-report-bucket";
        var reportKey = "reports/test-report.json";
        
        var testReport = new EnrollmentReportResponseDto
        {
            Title = "Test Report",
            GeneratedAt = DateTime.UtcNow,
            Summary = new EnrollmentReportSummaryDto
            {
                TotalEnrollments = 2,
                ActiveEnrollments = 2
            },
            Items = new List<EnrollmentReportItemDto>
            {
                new() { CourseCode = "MATH101", CourseTitle = "Math 101", StudentName = "John Doe" },
                new() { CourseCode = "SCI201", CourseTitle = "Science 201", StudentName = "Jane Smith" }
            }
        };

        var saveResult = await _s3Service!.SaveReportToS3Async(bucketName, reportKey, testReport);
        Assert.True(saveResult);

        var retrievedReport = await _s3Service.GetReportFromS3Async<EnrollmentReportResponseDto>(bucketName, reportKey);
        
        Assert.NotNull(retrievedReport);
        Assert.Equal(testReport.Summary.TotalEnrollments, retrievedReport.Summary.TotalEnrollments);
        Assert.Equal(testReport.Items.Count, retrievedReport.Items.Count);
    }

    [Fact]
    public async Task SaveEnrollmentDataWithMetadata_ShouldIncludeCustomMetadata()
    {
        var bucketName = "test-enrollment-metadata-bucket";
        var enrollmentKey = "enrollments/enrollment-with-metadata.json";
        
        var enrollmentData = new EnrollmentDto
        {
            Id = Guid.NewGuid(),
            StudentId = "student-123",
            CourseId = Guid.NewGuid(),
            EnrollmentDate = DateTime.UtcNow,
            Status = ProjectAthena.Data.Models.EnrollmentStatus.Active,
            Grade = 88.5m
        };

        var saveResult = await _s3Service!.SaveReportToS3Async(bucketName, enrollmentKey, enrollmentData);
        Assert.True(saveResult);

        var retrievedData = await _s3Service.GetReportFromS3Async<EnrollmentDto>(bucketName, enrollmentKey);
        
        Assert.NotNull(retrievedData);
        Assert.Equal(enrollmentData.Id, retrievedData.Id);
        Assert.Equal(enrollmentData.StudentId, retrievedData.StudentId);
        Assert.Equal(enrollmentData.Grade, retrievedData.Grade);
    }

    [Fact]
    public async Task ListReports_ShouldReturnCorrectKeys()
    {
        var bucketName = "test-list-bucket";
        var prefix = "reports/2024/";
        
        var reportKeys = new[]
        {
            "reports/2024/january-report.json",
            "reports/2024/february-report.json",
            "reports/2024/march-report.json"
        };

        foreach (var key in reportKeys)
        {
            var dummyData = new { ReportName = key, GeneratedAt = DateTime.UtcNow };
            await _s3Service!.SaveReportToS3Async(bucketName, key, dummyData);
        }

        var listedKeys = await _s3Service!.ListReportsAsync(bucketName, prefix);
        
        Assert.Equal(3, listedKeys.Count);
        Assert.All(reportKeys, key => Assert.Contains(key, listedKeys));
    }

    [Fact]
    public async Task DeleteReport_ShouldRemoveFromS3()
    {
        var bucketName = "test-delete-bucket";
        var reportKey = "reports/report-to-delete.json";
        
        var testData = new { Message = "This will be deleted" };
        
        var saveResult = await _s3Service!.SaveReportToS3Async(bucketName, reportKey, testData);
        Assert.True(saveResult);

        var deleteResult = await _s3Service.DeleteReportFromS3Async(bucketName, reportKey);
        Assert.True(deleteResult);

        var retrievedData = await _s3Service.GetReportFromS3Async<object>(bucketName, reportKey);
        Assert.Null(retrievedData);
    }

    [Fact]
    public async Task BulkReportStorage_ShouldHandleMultipleReports()
    {
        var bucketName = "test-bulk-reports-bucket";
        var reports = new List<(string Key, object Data)>();
        
        for (int i = 1; i <= 5; i++)
        {
            var report = new EnrollmentReportDto
            {
                CourseCode = $"COURSE{i}",
                CourseTitle = $"Course {i}",
                TotalEnrollments = i * 10,
                ActiveEnrollments = i * 8,
                CompletedEnrollments = i * 2,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow
            };
            
            reports.Add(($"bulk-reports/report-{i}.json", report));
        }

        var saveTasks = reports.Select(r => _s3Service!.SaveReportToS3Async(bucketName, r.Key, r.Data));
        var saveResults = await Task.WhenAll(saveTasks);
        
        Assert.All(saveResults, result => Assert.True(result));

        var listedKeys = await _s3Service!.ListReportsAsync(bucketName, "bulk-reports/");
        Assert.Equal(5, listedKeys.Count);

        var retrieveTasks = reports.Select(r => 
            _s3Service.GetReportFromS3Async<EnrollmentReportDto>(bucketName, r.Key));
        var retrieveResults = await Task.WhenAll(retrieveTasks);
        
        Assert.All(retrieveResults, result => Assert.NotNull(result));
        
        for (int i = 0; i < reports.Count; i++)
        {
            var originalReport = reports[i].Data as EnrollmentReportDto;
            var retrievedReport = retrieveResults[i];
            
            Assert.Equal(originalReport!.CourseTitle, retrievedReport!.CourseTitle);
            Assert.Equal(originalReport.TotalEnrollments, retrievedReport.TotalEnrollments);
        }
    }

    [Fact]
    public async Task ErrorHandling_ShouldHandleNonExistentObjects()
    {
        var bucketName = "test-error-bucket";
        var nonExistentKey = "reports/does-not-exist.json";

        var retrievedData = await _s3Service!.GetReportFromS3Async<object>(bucketName, nonExistentKey);
        Assert.Null(retrievedData);

        var deleteResult = await _s3Service.DeleteReportFromS3Async(bucketName, nonExistentKey);
        Assert.True(deleteResult);
    }
}