using aggregate_api.Application.Domain.Responses;
using aggregate_api.Core.Swagger.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace aggregate_api.Core.Swagger.Groups;

public class AggregationResponseGroup
{
    public void ApplyV1SuccessfulSendResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v1/aggregate/send"))
        {
            operation.Responses.Clear();
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(AggregationResponse), context.SchemaRepository),
                        Example = OpenApiHelper.CreateOpenApiObject(new AggregationResponse
                        {
                            CorrelationId = "CorrelationId linked to request",
                        })
                    }
                }
            });
        }
    }
    public void ApplyV1SuccessfulUnauthResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v1/aggregate/unauth-send"))
        {
            operation.Responses.Clear();
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(AggregationResponse), context.SchemaRepository),
                        Example = OpenApiHelper.CreateOpenApiObject(new AggregationResponse
                        {
                            CorrelationId = "CorrelationId linked to request",
                        })
                    }
                }
            });
        }
    }
    public void ApplyV1SuccessfulTempletedResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v1/aggregate/templated-send"))
        {
            operation.Responses.Clear();
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(AggregationResponse), context.SchemaRepository),
                        Example = OpenApiHelper.CreateOpenApiObject(new AggregationResponse
                        {
                            CorrelationId = "CorrelationId linked to request",
                        })
                    }
                }
            });
        }
    }
}