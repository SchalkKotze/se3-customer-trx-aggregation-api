namespace aggregate_api.Infrastructure.Contracts;

public interface IEnvironmentService
{
    // Api Configs
    
    string LogLevel { get; }
    
    // Token Configs
    
    string ResourceId { get; }
    string InstanceUrl { get; }
    string TenantId { get; }
    
    // External Transaction Sources Connection details
    string SourceBankConnection { get; }
    string SourceCreditConnection { get; }
    string SourceInvestmentConnection { get; }
}