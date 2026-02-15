using System.Text;
using aggregate_api.Application.Domain.Enums;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Infrastructure.Contracts;

namespace aggregate_api.Infrastructure;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    
    private readonly LogLevelEnum _logLevel;
    
    public LoggingService(ILogger<LoggingService> logger,
                          IEnvironmentService environmentService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logLevel = (LogLevelEnum)Enum.Parse(typeof(LogLevelEnum), environmentService.LogLevel);
    }

    public void LogCritical(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.CRITICAL)
        {
            _logger.LogCritical(message);
        }
    }
    public void LogCritical(string message, Exception ex)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.CRITICAL)
        {
            var stringBuilder = new StringBuilder();
                
            stringBuilder.Append(message);
            stringBuilder.Append(" - {ex}");
        
            _logger.LogCritical(stringBuilder.ToString(), ex);
        }
    }
    public void LogCritical(string message, ResponseModel responseModel)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.CRITICAL)
        {
            responseModel.AddException(message);
            
            _logger.LogCritical(message);
        }
    }
    public void LogCritical(string message, Exception ex, ResponseModel responseModel)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.CRITICAL)
        {
            var stringBuilder = new StringBuilder();
                
            stringBuilder.Append(message);
            stringBuilder.Append(" - {ex}");
        
            responseModel.AddException(message);
            
            _logger.LogCritical(stringBuilder.ToString(), ex);
        }
    }
    
    public void LogError(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.ERROR)
        {
            _logger.LogError(message);   
        }
    }
    public void LogError(string message, Exception ex)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.ERROR)
        {
            var stringBuilder = new StringBuilder();
                
            stringBuilder.Append(message);
            stringBuilder.Append(" - {ex}");
        
            _logger.LogError(stringBuilder.ToString(), ex);   
        }
    }
    public void LogError(string message, ResponseModel responseModel)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.ERROR)
        {
            responseModel.AddError(message);
            
            _logger.LogError(message);   
        }
    }
    public void LogError(string message, Exception ex, ResponseModel responseModel)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.ERROR)
        {
            var stringBuilder = new StringBuilder();
                
            stringBuilder.Append(message);
            stringBuilder.Append(" - {ex}");
            
            responseModel.AddError(message);
        
            _logger.LogError(stringBuilder.ToString(), ex);   
        }
    }

    public void LogWarning(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.WARNING)
        {
            _logger.LogWarning(message);   
        }
    }
    public void LogInformation(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.INFORMATION)
        {
            _logger.LogInformation(message);   
        }
    }
    public void LogDebug(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.DEBUG)
        {
            _logger.LogDebug(message);   
        }
    }
    public void LogTrace(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_logLevel >= LogLevelEnum.TRACE)
        {
            _logger.LogTrace(message);   
        }
    }
}