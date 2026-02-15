using Microsoft.OpenApi.Any;
using Newtonsoft.Json;

namespace aggregate_api.Core.Swagger.Helpers;

public static class OpenApiHelper
{
    public static IOpenApiAny CreateOpenApiString(string response)
    {
        return new OpenApiString(response);
    }
    
    public static IOpenApiAny CreateOpenApiObject<T>(T example)
    {
        var json = JsonConvert.SerializeObject(example, Formatting.Indented);
        return new OpenApiString(json);
    }
    
    public static IOpenApiAny CreateOpenApiArray<T>(IList<T> items)
    {
        var json = JsonConvert.SerializeObject(items, Formatting.Indented);
        return new OpenApiString(json);
    }
}