using aggregate_api.Application.Domain.Models;

namespace aggregate_api.Infrastructure.Contracts;

public interface IAwsBucketService
{
    Task<ResponseModel> PutObjectAsync(string bucketName, string objectKey, string contentType, byte[] FileBytes, CancellationToken token);
    Task<ResponseModel> PutObjectAsync(string bucketName, string objectKey, string contentType, string contentBody, CancellationToken token);
    Task<ResponseModel<MemoryStream>> GetObjectAsync(string bucketName, string objectKey, CancellationToken token);
    Task<ResponseModel> DeleteObjectAsync(string bucketName, string objectKey, CancellationToken token);
}