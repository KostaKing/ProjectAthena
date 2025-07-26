using Amazon.S3;
using Amazon.S3.Model;

namespace ProjectAthena.Tests;

public class MockS3Client
{
    private readonly Dictionary<string, Dictionary<string, string>> _buckets = new();
    
    public Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
    {
        if (!_buckets.ContainsKey(request.BucketName))
        {
            _buckets[request.BucketName] = new Dictionary<string, string>();
        }
        
        _buckets[request.BucketName][request.Key] = request.ContentBody;
        
        return Task.FromResult(new PutObjectResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            ETag = $"\"{Guid.NewGuid()}\""
        });
    }
    
    public Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = default)
    {
        if (!_buckets.ContainsKey(request.BucketName) || 
            !_buckets[request.BucketName].ContainsKey(request.Key))
        {
            throw new AmazonS3Exception("Object not found");
        }
        
        var content = _buckets[request.BucketName][request.Key];
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        
        return Task.FromResult(new GetObjectResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            ResponseStream = stream
        });
    }

    public Task<GetBucketLocationResponse> GetBucketLocationAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        if (!_buckets.ContainsKey(bucketName))
        {
            throw new AmazonS3Exception("Bucket not found") { StatusCode = System.Net.HttpStatusCode.NotFound };
        }
        
        return Task.FromResult(new GetBucketLocationResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            Location = "us-east-1"
        });
    }

    public Task<PutBucketResponse> PutBucketAsync(PutBucketRequest request, CancellationToken cancellationToken = default)
    {
        if (!_buckets.ContainsKey(request.BucketName))
        {
            _buckets[request.BucketName] = new Dictionary<string, string>();
        }
        
        return Task.FromResult(new PutBucketResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request, CancellationToken cancellationToken = default)
    {
        if (_buckets.ContainsKey(request.BucketName) && _buckets[request.BucketName].ContainsKey(request.Key))
        {
            _buckets[request.BucketName].Remove(request.Key);
        }
        
        return Task.FromResult(new DeleteObjectResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.NoContent
        });
    }

    public Task<ListObjectsV2Response> ListObjectsV2Async(ListObjectsV2Request request, CancellationToken cancellationToken = default)
    {
        var objects = new List<S3Object>();
        
        if (_buckets.ContainsKey(request.BucketName))
        {
            var matchingKeys = _buckets[request.BucketName].Keys
                .Where(key => string.IsNullOrEmpty(request.Prefix) || key.StartsWith(request.Prefix))
                .ToList();
            
            objects.AddRange(matchingKeys.Select(key => new S3Object
            {
                Key = key,
                BucketName = request.BucketName,
                LastModified = DateTime.UtcNow,
                Size = _buckets[request.BucketName][key].Length
            }));
        }
        
        return Task.FromResult(new ListObjectsV2Response
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            S3Objects = objects
        });
    }

}