using aggregate_api.Core.Swagger.Groups;
using aggregate_api.Core.Swagger.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace aggregate_api.Core.Swagger.Filters;

public class SwaggerResponseFilter : IOperationFilter
{
    private readonly AggregationResponseGroup _aggregationResponseGroup;
    private readonly TokenResponseGroup _tokenResponseGroup;
    
    public SwaggerResponseFilter(AggregationResponseGroup aggregationResponseGroup,
                                 TokenResponseGroup tokenResponseGroup)
    {
        _aggregationResponseGroup = aggregationResponseGroup ?? throw new ArgumentNullException(nameof(aggregationResponseGroup));
        _tokenResponseGroup = tokenResponseGroup ?? throw new ArgumentNullException(nameof(tokenResponseGroup));
    }
    
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpoint = context.ApiDescription.RelativePath;
        var httpMethod = context.ApiDescription.HttpMethod;
        
        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(httpMethod))
        {
            _aggregationResponseGroup.ApplyV1SuccessfulSendResponse(operation, context, endpoint);
            _aggregationResponseGroup.ApplyV1SuccessfulUnauthResponse(operation, context, endpoint);
            _aggregationResponseGroup.ApplyV1SuccessfulTempletedResponse(operation, context, endpoint);
            
            _tokenResponseGroup.ApplySuccessfulResponse(operation, context, endpoint);
            
            AddCommonResponses(operation, context);
        }
    }

    private void AddCommonResponses(OpenApiOperation operation, OperationFilterContext context)
    {
        // Only add if not already present to avoid duplicate-key exceptions.
        operation.Responses.TryAdd("400", new OpenApiResponse
        {
            Description = "Bad Request",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), context.SchemaRepository)
                }
            }
        });

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("405", new OpenApiResponse { Description = "Method Not Allowed" });
        operation.Responses.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });
    }
}