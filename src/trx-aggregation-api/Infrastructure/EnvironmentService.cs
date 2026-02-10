using aggregate_api.Infrastructure.Contracts;

namespace aggregate_api.Infrastructure;

public class EnvironmentService : IEnvironmentService
{
    // Api Configs
    
    public string LogLevel => GetEnvironmentVariable("LOG_LEVEL");
    
    // Token Configs
    
    public string ResourceId => GetEnvironmentVariable("EMAIL_API_AAD_RESOURCE_ID");
    public string InstanceUrl => GetEnvironmentVariable("EMAIL_API_AAD_INSTANCE");
    public string TenantId => GetEnvironmentVariable("EMAIL_API_AAD_TENANT_ID");
    
    // SQS Configs
    
    public string SqsRegion => GetEnvironmentVariable("SQS_REGION");
    public string SqsEmailSendQueue => GetEnvironmentVariable("SQS_EMAIL_SEND_QUEUE");
    
    // S3 Configs
    
    public string BucketRegion => GetEnvironmentVariable("BUCKET_REGION");
    public string RetailBucketName => GetEnvironmentVariable("RETAIL_BUCKET_NAME");
    public string BusinessBucketName => GetEnvironmentVariable("BUSINESS_BUCKET_NAME");
    
    private string GetEnvironmentVariable(string environmentVariableName)
    {
        return Environment.GetEnvironmentVariable(environmentVariableName)
               ?? throw new Exception($"Environment variable '{environmentVariableName}' not found");
    }
}