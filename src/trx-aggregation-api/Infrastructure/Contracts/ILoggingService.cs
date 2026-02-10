using aggregate_api.Application.Domain.Models;

namespace aggregate_api.Infrastructure.Contracts;

public interface ILoggingService
{
    void LogCritical(string message);
    void LogCritical(string message, Exception ex);
    void LogCritical(string message, ResponseModel responseModel);
    void LogCritical(string message, Exception ex, ResponseModel responseModel);
    
    void LogError(string message);
    void LogError(string message, Exception ex);
    void LogError(string message, ResponseModel responseModel);
    void LogError(string message, Exception ex, ResponseModel responseModel);

    void LogWarning(string message);
    void LogInformation(string message);
    void LogDebug(string message);
    void LogTrace(string message);
}