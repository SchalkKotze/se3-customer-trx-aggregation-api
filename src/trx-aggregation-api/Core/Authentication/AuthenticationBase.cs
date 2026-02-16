using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace aggregate_api.Core.Authentication;

public static class AuthenticationBase
{
    public static IServiceCollection UseCustomJwtBearer(this IServiceCollection services)
    {
        var aadResourceId = Environment.GetEnvironmentVariable("AGGREGATE_API_AAD_RESOURCE_ID");
        var aadInstance = Environment.GetEnvironmentVariable("AGGREGATE_API_AAD_INSTANCE");
        var aadTenantId = Environment.GetEnvironmentVariable("AGGREGATE_API_AAD_TENANT_ID");
        var htmlRoles = Environment.GetEnvironmentVariable("AGGREGATE_API_HTML_ROLES");
        var templateRoles = Environment.GetEnvironmentVariable("AGGREGATE_API_TEMPLATE_ROLES");

        if (string.IsNullOrWhiteSpace(aadResourceId) || string.IsNullOrWhiteSpace(aadInstance) || string.IsNullOrWhiteSpace(aadTenantId))
        {
            throw new ArgumentException("JWT Bearer token environment variable is missing or invalid");
        }
        
        if (string.IsNullOrWhiteSpace(htmlRoles))
        {
            throw new ArgumentException("Role environment variable is missing or invalid");
        }

       
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.Audience = aadResourceId;
                opt.Authority = $"{aadInstance}{aadTenantId}";

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = new []
                    {
                        $"https://sts.windows.net/{aadTenantId}/",
                        $"https://sts.windows.net/{aadTenantId}/"
                    }.ToList(),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
                opt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<Microsoft.Extensions.Logging.ILogger<object>>();
                        logger.LogError(ctx.Exception, "JWT Auth Failed");
                        return Task.CompletedTask;
                    }
                };
            });
        
        return services;
    }
}