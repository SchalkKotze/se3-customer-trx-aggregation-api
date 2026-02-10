namespace aggregate_api.Infrastructure.Contracts;

public interface IEnvironmentService
{
    // Api Configs
    
    string LogLevel { get; }
    
    // Token Configs
    
    string ResourceId { get; }
    string InstanceUrl { get; }
    string TenantId { get; }
    
    // SQS Configs
    
    string SqsRegion { get; }
    string SqsEmailSendQueue { get; }
    
    // S3 Configs
    
    string BucketRegion { get; }
    string RetailBucketName { get; }
    string BusinessBucketName { get; }
}