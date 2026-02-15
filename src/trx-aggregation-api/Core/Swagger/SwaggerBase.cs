using aggregate_api.Core.Swagger.Filters;
using aggregate_api.Core.Swagger.Groups;
using Microsoft.OpenApi.Models;

namespace aggregate_api.Core.Swagger;

public static class SwaggerBase
{
    public static IServiceCollection InjectSwaggerFilters(this IServiceCollection services)
    {
        services.AddSingleton<AggregationResponseGroup>();
        services.AddSingleton<TokenResponseGroup>();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Customer Aggregation API V1",
                Description = "Endpoints for version 1 of Customer Aggregation API"
            });

            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Version = "v2",
                Title = "Customer Aggregation API V2",
                Description = "Endpoints for version 2 of Customer Aggregation API"
            });

            c.SwaggerDoc("v3", new OpenApiInfo
            {
                Version = "v3",
                Title = "Customer Aggregation API V3",
                Description = "Endpoints for version 3 of Customer Aggregation API"
            });
            
            c.SwaggerDoc("v4", new OpenApiInfo
            {
                Version = "v4",
                Title = "Customer Aggregation API V4",
                Description = "Endpoints for version 4 of Customer Aggregation API"
            });

            c.CustomSchemaIds(type => type.FullName);
            c.SupportNonNullableReferenceTypes();
            c.OperationFilter<SwaggerResponseFilter>();
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });
        
        return services;
    }
}