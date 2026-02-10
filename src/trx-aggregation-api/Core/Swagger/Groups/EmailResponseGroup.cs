using aggregate_api.Application.Domain.Responses;
using aggregate_api.Core.Swagger.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace aggregate_api.Core.Swagger.Groups;

public class EmailResponseGroup
{
    public void ApplyV1SuccessfulSendResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v1/emails/send"))
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
        if (endpoint.Equals("api/v1/emails/unauth-send"))
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
        if (endpoint.Equals("api/v1/emails/templated-send"))
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
    
    public void ApplyV2SuccessfulSendResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v2/emails/send"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Description = "Tracking requests and duplication management",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
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
    public void ApplyV2SuccessfulTempletedResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v2/emails/templated-send"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Description = "Tracking requests and duplication management",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
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
    
    public void ApplyV3SuccessfulSendResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v3/emails/send"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Description = "Tracking requests and duplication management",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-source-system",
                In = ParameterLocation.Header,
                Description = "This will be used to determine the calling system",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
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
    public void ApplyV3SuccessfulTempletedResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v3/emails/templated-send"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Description = "Tracking requests and duplication management",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-source-system",
                In = ParameterLocation.Header,
                Description = "This will be used to determine the calling system",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
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
    
    public void ApplyV4SuccessfulSendResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v4/emails/send"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Description = "Tracking requests and duplication management",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-source-system",
                In = ParameterLocation.Header,
                Description = "This will be used to determine the calling system",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
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
    public void ApplyV4SuccessfulTempletedResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v4/emails/templated-send"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Description = "Tracking requests and duplication management",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-source-system",
                In = ParameterLocation.Header,
                Description = "This will be used to determine the calling system",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
            
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