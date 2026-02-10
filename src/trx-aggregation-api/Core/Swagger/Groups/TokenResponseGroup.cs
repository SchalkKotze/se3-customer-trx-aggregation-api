using aggregate_api.Application.Domain.Responses;
using aggregate_api.Core.Swagger.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace aggregate_api.Core.Swagger.Groups;

public class TokenResponseGroup
{
    public void ApplySuccessfulResponse(OpenApiOperation operation, OperationFilterContext context, string endpoint)
    {
        if (endpoint.Equals("api/v1/tokens"))
        {
            operation.Responses.Clear();
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(TokenResponse), context.SchemaRepository),
                        Example = OpenApiHelper.CreateOpenApiObject(new TokenResponse
                        {
                            TokenType = "Bearer",
                            ExpiresOn = new DateTimeOffset(),
                            AccessToken = "JWT token used for access"
                        })
                    }
                }
            });
        }
    }
}