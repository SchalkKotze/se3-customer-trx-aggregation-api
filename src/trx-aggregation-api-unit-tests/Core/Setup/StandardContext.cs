using System.Diagnostics;
/*using email_api.Core.AutoMapper;
using email_api.Core.FluentValidators;
using email_api.Infrastructure;
using email_api.Infrastructure.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace email_api_unit_tests.Core.Setup;

public class StandardContext
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    public void Setup(ITestOutputHelper outputHelper, Action<ServiceCollection> configure)
    {
        var services = new ServiceCollection();
        
        SetupEnvironmentVariables();
        
        // Setup Logging
        services.AddSingleton(outputHelper);
        services.AddLogging(config =>
        {
            config.ClearProviders();
            config.SetMinimumLevel(LogLevel.Trace);
        });
        
        // Register AutoMapper Profiles
        services.InjectAutoMapperProfiles();

        // Register Fluent Validators
        services.InjectFluentValidators();
        
        // Register Services
        services
            .AddSingleton<IEnvironmentService, EnvironmentService>()
            .AddSingleton<ILoggingService, LoggingService>();
        
        // Register Metrics
        services.AddTransient<Stopwatch>();
        
        configure.Invoke(services);

        ServiceProvider = services.BuildServiceProvider();
    }
    
    private void SetupEnvironmentVariables()
    {
        // Api Configs
        
        Environment.SetEnvironmentVariable("SWAGGER_KILLSWITCH", "false");
        Environment.SetEnvironmentVariable("LOG_LEVEL", "TRACE");
        
        // Token Configs
        
        Environment.SetEnvironmentVariable("EMAIL_API_AAD_RESOURCE_ID", "api://612132d6-7f1f-4b52-b5a8-9f6b733ce0da");
        Environment.SetEnvironmentVariable("EMAIL_API_AAD_INSTANCE", "https://login.microsoftonline.com/");
        Environment.SetEnvironmentVariable("EMAIL_API_AAD_TENANT_ID", "a428b46f-c29b-4a6c-85f1-8e05c10b6671");
    
        // SQS Configs
        
        Environment.SetEnvironmentVariable("SQS_CHUNK_SIZE", "100");
        Environment.SetEnvironmentVariable("SQS_EMAIL_SEND_QUEUE", "TEST");
        
        // S3 Configs
        
        Environment.SetEnvironmentVariable("EMAIL_BUCKET_NAME", "TEST");
    }
}
*/