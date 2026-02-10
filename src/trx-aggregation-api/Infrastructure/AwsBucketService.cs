using System.Diagnostics;
using System.Net;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Infrastructure.Contracts;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace aggregate_api.Infrastructure;

public class AwsBucketService : IAwsBucketService
{
    private readonly ILoggingService _loggingService;
    private readonly AmazonS3Client _s3Client;
    
    private readonly Stopwatch _stopwatch;
    
    public AwsBucketService(ILoggingService loggingService,
                            IEnvironmentService environmentService,
                            Stopwatch stopwatch)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        var _environmentService = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        _stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        
        _s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(_environmentService.SqsRegion));
    }

    public async Task<ResponseModel> PutObjectAsync(string bucketName, string objectKey, string contentType, byte[] FileBytes, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AwsBucketService", "PutObjectAsync"));
        
        _stopwatch.Reset();
        _stopwatch.Start();
        
        var r = new ResponseModel();
        using (MemoryStream stream = new MemoryStream(FileBytes))
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = contentType
            };
                
            var putObjectResponse = await _s3Client.PutObjectAsync(putObjectRequest, token);

            if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _loggingService.LogCritical($"Failed to send object to bucket. Bucket Name: {bucketName}", r);
            }
        }
        
        _stopwatch.Stop();
        
        _loggingService.LogDebug(LoggingMessages.Stopwatch("AwsBucketService", "PutObjectAsync", _stopwatch.Elapsed));

        return r;
    }
    public async Task<ResponseModel> PutObjectAsync(string bucketName, string objectKey, string contentType, string contentBody, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AwsBucketService", "PutObjectAsync"));
        
        _stopwatch.Reset();
        _stopwatch.Start();
        
        var r = new ResponseModel<string>(objectKey);
        var putObjectRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            ContentType = contentType,
            ContentBody = contentBody
        };
        
        var putObjectResponse = await _s3Client.PutObjectAsync(putObjectRequest, token);
        
        if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _loggingService.LogCritical($"Failed to send object to bucket. Bucket Name: {bucketName}", r);
        }
        
        _stopwatch.Stop();
        
        _loggingService.LogDebug(LoggingMessages.Stopwatch("AwsBucketService", "PutObjectAsync", _stopwatch.Elapsed));

        return r;
    }
    public async Task<ResponseModel<MemoryStream>> GetObjectAsync(string bucketName, string objectKey, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AwsBucketService", "GetObjectAsync"));
        
        _stopwatch.Reset();
        _stopwatch.Start();

        var r = new ResponseModel<MemoryStream>(new MemoryStream());
        var getObjectRequest = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };

        using var getObjectResponse = await _s3Client.GetObjectAsync(getObjectRequest, token);
        await getObjectResponse.ResponseStream.CopyToAsync(r.Data);

        if (getObjectResponse.HttpStatusCode != HttpStatusCode.OK || r.Data.Length <= 0)
        {
            _loggingService.LogCritical($"Failed to receive object from bucket. Bucket Name: {bucketName}", r);
        }
        
        _stopwatch.Stop();
        
        _loggingService.LogDebug(LoggingMessages.Stopwatch("AwsBucketService", "GetObjectAsync", _stopwatch.Elapsed));
        
        return r;
    }
    public async Task<ResponseModel> DeleteObjectAsync(string bucketName, string objectKey, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AwsBucketService", "DeleteObjectAsync"));
        
        _stopwatch.Reset();
        _stopwatch.Start();
        
        var r = new ResponseModel();
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };
        
        var deleteObjectResponse = await _s3Client.DeleteObjectAsync(deleteObjectRequest, token);
        
        if (deleteObjectResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _loggingService.LogCritical($"Failed to delete object on bucket. Bucket Name: {bucketName}", r);
        }
        
        _stopwatch.Stop();
        
        _loggingService.LogDebug(LoggingMessages.Stopwatch("AwsBucketService", "DeleteObjectAsync", _stopwatch.Elapsed));
        
        return r;
    }
}