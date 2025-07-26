using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ProjectAthena.Tests;

public interface IAwsS3IntegrationService
{
    Task<bool> SaveReportToS3Async<T>(string bucketName, string key, T data, CancellationToken cancellationToken = default);
    Task<T?> GetReportFromS3Async<T>(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteReportFromS3Async(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<List<string>> ListReportsAsync(string bucketName, string prefix = "", CancellationToken cancellationToken = default);
    Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default);
}

public class AwsS3IntegrationService : IAwsS3IntegrationService
{
    private readonly MockS3Client _s3Client;
    private readonly ILogger<AwsS3IntegrationService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AwsS3IntegrationService(MockS3Client s3Client, ILogger<AwsS3IntegrationService> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<bool> SaveReportToS3Async<T>(string bucketName, string key, T data, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(bucketName, cancellationToken);

            var jsonData = JsonSerializer.Serialize(data, _jsonOptions);
            
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentBody = jsonData,
                ContentType = "application/json",
                Metadata = 
                {
                    ["data-type"] = typeof(T).Name,
                    ["created-at"] = DateTime.UtcNow.ToString("O"),
                    ["version"] = "1.0"
                },
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);
            
            var success = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            
            if (success)
            {
                _logger.LogInformation("Successfully saved data to S3. Bucket: {BucketName}, Key: {Key}, ETag: {ETag}", 
                    bucketName, key, response.ETag);
            }
            else
            {
                _logger.LogWarning("Failed to save data to S3. Bucket: {BucketName}, Key: {Key}, StatusCode: {StatusCode}", 
                    bucketName, key, response.HttpStatusCode);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving data to S3. Bucket: {BucketName}, Key: {Key}", bucketName, key);
            return false;
        }
    }

    public async Task<T?> GetReportFromS3Async<T>(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);
            
            using var reader = new StreamReader(response.ResponseStream);
            var jsonData = await reader.ReadToEndAsync();
            
            var data = JsonSerializer.Deserialize<T>(jsonData, _jsonOptions);
            
            _logger.LogInformation("Successfully retrieved data from S3. Bucket: {BucketName}, Key: {Key}", 
                bucketName, key);
            
            return data;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Object not found in S3. Bucket: {BucketName}, Key: {Key}", bucketName, key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data from S3. Bucket: {BucketName}, Key: {Key}", bucketName, key);
            return default;
        }
    }

    public async Task<bool> DeleteReportFromS3Async(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            
            var success = response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            
            if (success)
            {
                _logger.LogInformation("Successfully deleted object from S3. Bucket: {BucketName}, Key: {Key}", 
                    bucketName, key);
            }
            else
            {
                _logger.LogWarning("Failed to delete object from S3. Bucket: {BucketName}, Key: {Key}, StatusCode: {StatusCode}", 
                    bucketName, key, response.HttpStatusCode);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting object from S3. Bucket: {BucketName}, Key: {Key}", bucketName, key);
            return false;
        }
    }

    public async Task<List<string>> ListReportsAsync(string bucketName, string prefix = "", CancellationToken cancellationToken = default)
    {
        try
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix,
                MaxKeys = 1000
            };

            var response = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);
            
            var keys = response.S3Objects.Select(obj => obj.Key).ToList();
            
            _logger.LogInformation("Successfully listed {Count} objects from S3. Bucket: {BucketName}, Prefix: {Prefix}", 
                keys.Count, bucketName, prefix);
            
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing objects from S3. Bucket: {BucketName}, Prefix: {Prefix}", bucketName, prefix);
            return new List<string>();
        }
    }

    public async Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetBucketLocationAsync(bucketName, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if bucket exists. BucketName: {BucketName}", bucketName);
            return false;
        }
    }

    public async Task<bool> CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await BucketExistsAsync(bucketName, cancellationToken))
            {
                _logger.LogInformation("Bucket already exists: {BucketName}", bucketName);
                return true;
            }

            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            var response = await _s3Client.PutBucketAsync(putBucketRequest, cancellationToken);
            
            var success = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            
            if (success)
            {
                _logger.LogInformation("Successfully created bucket: {BucketName}", bucketName);
                
                await Task.Delay(1000, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Failed to create bucket: {BucketName}, StatusCode: {StatusCode}", 
                    bucketName, response.HttpStatusCode);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bucket: {BucketName}", bucketName);
            return false;
        }
    }

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        if (!await BucketExistsAsync(bucketName, cancellationToken))
        {
            await CreateBucketAsync(bucketName, cancellationToken);
        }
    }
}

public static class AwsS3ServiceCollectionExtensions
{
    public static IServiceCollection AddAwsS3Integration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<MockS3Client>();
        services.AddScoped<IAwsS3IntegrationService, AwsS3IntegrationService>();

        return services;
    }
}