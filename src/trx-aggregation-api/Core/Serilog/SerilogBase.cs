using Amazon;
using Amazon.Runtime;
using AWS.Logger;
using AWS.Logger.SeriLog;
using Serilog;
using Serilog.Formatting.Compact;
namespace aggregate_api.Core.Serilog;

public static class SerilogBase
{
    public static void InjectSerilog(this WebApplicationBuilder builder)
    {
        var credentials = FallbackCredentialsFactory.GetCredentials();
        var awsLogGroup = Environment.GetEnvironmentVariable("AWS_LOG_GROUP_NAME");
        
        var awsConfiguration = new AWSLoggerConfig
        {
            Region = RegionEndpoint.AFSouth1.SystemName,
            LogGroup = awsLogGroup,
            Credentials = credentials
        };

        builder.Host.UseSerilog((ctx, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(ctx.Configuration)
                .WriteTo.AWSSeriLog(configuration: awsConfiguration, textFormatter: new RenderedCompactJsonFormatter())
                .WriteTo.Console();
        });
    }
}