using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace aggregate_api.Core.Filters;

public class CorrelationIdFilter : IActionFilter
{
    private const string CorrelationIdHeader = "x-correlation-id";
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) 
            || StringValues.IsNullOrEmpty(correlationId) 
            || !Guid.TryParse(correlationId, out _))
        {
            context.Result = new BadRequestObjectResult($"Invalid or missing required header: {CorrelationIdHeader}");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        
    }
}