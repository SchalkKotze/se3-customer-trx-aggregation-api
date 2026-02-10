using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace aggregate_api.Core.Authentication;

public static class AuthenticationBase
{
    public static IServiceCollection UseCustomJwtBearer(this IServiceCollection services)
    {
        var aadResourceId = Environment.GetEnvironmentVariable("EMAIL_API_AAD_RESOURCE_ID");
        var aadInstance = Environment.GetEnvironmentVariable("EMAIL_API_AAD_INSTANCE");
        var aadTenantId = Environment.GetEnvironmentVariable("EMAIL_API_AAD_TENANT_ID");
        var htmlRoles = Environment.GetEnvironmentVariable("EMAIL_API_HTML_ROLES");
        var templateRoles = Environment.GetEnvironmentVariable("EMAIL_API_TEMPLATE_ROLES");

        if (string.IsNullOrWhiteSpace(aadResourceId) || string.IsNullOrWhiteSpace(aadInstance) || string.IsNullOrWhiteSpace(aadTenantId))
        {
            throw new ArgumentException("JWT Bearer token environment variable is missing or invalid");
        }
        
        if (string.IsNullOrWhiteSpace(htmlRoles))
        {
            throw new ArgumentException("Role environment variable is missing or invalid");
        }

        services
            .AddAuthorization(options =>
            {
                options.AddPolicy("HtmlPolicy", policy =>
                {
                    if (!string.IsNullOrEmpty(htmlRoles))
                    {
                        var roles = htmlRoles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        policy.RequireRole(roles);
                    }
                });
                
                options.AddPolicy("TemplatedPolicy", policy =>
                {
                    if (!string.IsNullOrEmpty(templateRoles))
                    {
                        var roles = templateRoles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        policy.RequireRole(roles);
                    }
                });
            });
        
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.Audience = aadResourceId;
                opt.Authority = $"{aadInstance}{aadTenantId}";
            });
        
        return services;
    }
}