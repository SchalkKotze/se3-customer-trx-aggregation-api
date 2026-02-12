using aggregate_api.Infrastructure.Contracts;

namespace aggregate_api.Infrastructure;

public class EnvironmentService : IEnvironmentService
{
    // Api Configs
    
    public string LogLevel => GetEnvironmentVariable("LOG_LEVEL");
    
    // Token Configs
    
    public string ResourceId => GetEnvironmentVariable("AGGREGATE_API_AAD_RESOURCE_ID");
    public string InstanceUrl => GetEnvironmentVariable("AGGREGATE_API_AAD_INSTANCE");
    public string TenantId => GetEnvironmentVariable("AGGREGATE_API_AAD_TENANT_ID");
    
    // External TransactionSources Configs
    
    public string SourceBankConnection => GetEnvironmentVariable("SOURCE_BANK");
    public string SourceCreditConnection => GetEnvironmentVariable("SOURCE_CREDIT");
    public string SourceInvestmentConnection => GetEnvironmentVariable("SOURCE_INVESTMENT");
    
    private string GetEnvironmentVariable(string environmentVariableName)
    {
        return Environment.GetEnvironmentVariable(environmentVariableName)
               ?? throw new Exception($"Environment variable '{environmentVariableName}' not found");
    }
}