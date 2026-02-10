using aggregate_api.Core.Swagger.Groups;
using aggregate_api.Core.Swagger.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace aggregate_api.Core.Swagger.Filters;

public class SwaggerResponseFilter : IOperationFilter
{
    private readonly EmailResponseGroup _emailResponseGroup;
    private readonly TokenResponseGroup _tokenResponseGroup;
    
    public SwaggerResponseFilter(EmailResponseGroup emailResponseGroup,
                                 TokenResponseGroup tokenResponseGroup)
    {
        _emailResponseGroup = emailResponseGroup ?? throw new ArgumentNullException(nameof(emailResponseGroup));
        _tokenResponseGroup = tokenResponseGroup ?? throw new ArgumentNullException(nameof(tokenResponseGroup));
    }
    
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpoint = context.ApiDescription.RelativePath;
        var httpMethod = context.ApiDescription.HttpMethod;
        
        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(httpMethod))
        {
            _emailResponseGroup.ApplyV1SuccessfulSendResponse(operation, context, endpoint);
            _emailResponseGroup.ApplyV1SuccessfulUnauthResponse(operation, context, endpoint);
            _emailResponseGroup.ApplyV1SuccessfulTempletedResponse(operation, context, endpoint);
            
            _emailResponseGroup.ApplyV2SuccessfulSendResponse(operation, context, endpoint);
            _emailResponseGroup.ApplyV2SuccessfulTempletedResponse(operation, context, endpoint);
            
            _emailResponseGroup.ApplyV3SuccessfulSendResponse(operation, context, endpoint);
            _emailResponseGroup.ApplyV3SuccessfulTempletedResponse(operation, context, endpoint);
            
            _emailResponseGroup.ApplyV4SuccessfulSendResponse(operation, context, endpoint);
            _emailResponseGroup.ApplyV4SuccessfulTempletedResponse(operation, context, endpoint);
            
            _tokenResponseGroup.ApplySuccessfulResponse(operation, context, endpoint);
            
            AddCommonResponses(operation, context);
        }
    }

    private void AddCommonResponses(OpenApiOperation operation, OperationFilterContext context)
    {
        var Errors = new List<string> { "Validation errors" };
        
        operation.Responses.Add("400", new OpenApiResponse
        {
            Description = "Bad Request",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(List<string>), context.SchemaRepository),
                    Example = OpenApiHelper.CreateOpenApiArray(Errors)
                }
            }
        });
        
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("405", new OpenApiResponse { Description = "Method Not Allowed" });
        operation.Responses.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });
    }
}